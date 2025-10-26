
using System.Net;
using Microsoft.Extensions.Logging;
using UserService.BO.DTO;
using UserService.BO.Entities;
using UserService.BO.Enums;
using UserService.BO.Exceptions;
using UserService.Repository;

namespace UserService.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<bool> Create(UserRequest request)
        {
            try
            {
                var userExisting = await _userRepository.FindByEmail(request.Email);
                if (userExisting != null) throw new AppException("Email is already in use.", HttpStatusCode.BadRequest);
                request.Password = _passwordHasher.HashPassword(request.Password);
                var result = await _userRepository.AddUser(new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    Phone = request.Phone,
                    Password = request.Password,
                    Avatar = request.Avatar,
                    RoleName = request.RoleName,
                    Status = Status.ACTIVE,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                return result > 0 ? true : false;
            }
            catch (AppException aex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var userExisting = await _userRepository.FindById(id);
                if (userExisting == null) throw new AppException("User not found.", HttpStatusCode.NotFound);
                var result = await _userRepository.DeleteUser(userExisting);
                return result > 0 ? true : false;
            }
            catch (AppException aex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<PagedResult<UserResponse>> GetAll(string? query, RoleName? roleName, Status? status, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, pageSize);

                var paged = await _userRepository.FindAll(query, roleName, status, pageNumber, pageSize);

                return new PagedResult<UserResponse>
                {
                    Items = paged.Items.Select(user => new UserResponse
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DateOfBirth = user.DateOfBirth,
                        Phone = user.Phone,
                        Avatar = user.Avatar,
                        RoleName = user.RoleName,
                        Status = user.Status,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    }).ToList(),
                    TotalCount = paged.TotalCount,
                    PageNumber = paged.PageNumber,
                    PageSize = paged.PageSize
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", System.Net.HttpStatusCode.InternalServerError);
            }
        }

        public async Task<UserResponse?> GetById(int id)
        {
            try
            {
                var userExisting = await _userRepository.FindById(id);
                if (userExisting == null) throw new AppException("User not found.", HttpStatusCode.NotFound);
                return new UserResponse
                {
                    Id = userExisting.Id,
                    UserName = userExisting.UserName,
                    Email = userExisting.Email,
                    FirstName = userExisting.FirstName,
                    LastName = userExisting.LastName,
                    DateOfBirth = userExisting.DateOfBirth,
                    Phone = userExisting.Phone,
                    Avatar = userExisting.Avatar,
                    RoleName = userExisting.RoleName,
                    Status = userExisting.Status,
                    CreatedAt = userExisting.CreatedAt,
                    UpdatedAt = userExisting.UpdatedAt
                };
            }
            catch (AppException aex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> Update(int id, UserUpdateRequest request)
        {
            try
            {
                var userExisting = await _userRepository.FindById(id);
                if (userExisting == null) throw new AppException("User not found.", HttpStatusCode.NotFound);
                if (request.FirstName != null) userExisting.FirstName = request.FirstName;
                if (request.LastName != null) userExisting.LastName = request.LastName;
                if (request.DateOfBirth != null) userExisting.DateOfBirth = request.DateOfBirth;
                if (request.Phone != null) userExisting.Phone = request.Phone;
                if (request.Avatar != null) userExisting.Avatar = request.Avatar;
                if (request.Status != null) userExisting.Status = request.Status.Value;
                userExisting.UpdatedAt = DateTime.UtcNow;
                var result = await _userRepository.UpdateUser(userExisting);
                return result > 0 ? true : false;
            }
            catch (AppException aex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: ${ex.Message}");
                throw new AppException("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }
    }
}