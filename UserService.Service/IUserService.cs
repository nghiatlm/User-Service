
using UserService.BO.DTO;
using UserService.BO.Enums;

namespace UserService.Service
{
    public interface IUserService
    {
        Task<UserResponse?> GetById(int id);
        Task<bool> Create(UserRequest request);
        Task<bool> Delete(int id);
        Task<bool> Update(int id, UserUpdateRequest request);
        Task<PagedResult<UserResponse>> GetAll(string? query, RoleName? roleName, Status? status, int pageNumber = 1, int pageSize = 10);
    }
}