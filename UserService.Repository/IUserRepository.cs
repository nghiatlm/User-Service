
using UserService.BO.DTO;
using UserService.BO.Entities;
using UserService.BO.Enums;

namespace UserService.Repository
{
    public interface IUserRepository
    {
        Task<User?> FindById(int id);
        Task<User?> FindByEmail(string email);
        Task<int> AddUser(User user);
        Task<int> UpdateUser(User user);
        Task<int> DeleteUser(User user);
        Task<PagedResult<User>> FindAll(string? query, RoleName? roleName, Status? status, int pageNumber, int pageSize);
    }
}