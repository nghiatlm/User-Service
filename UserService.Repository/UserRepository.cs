using Microsoft.EntityFrameworkCore;
using UserService.BO.DTO;
using UserService.BO.Entities;
using UserService.BO.Enums;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<int> AddUser(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteUser(User user)
        {
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<User>> FindAll(string? query, RoleName? roleName, Status? status, int pageNumber, int pageSize)
        {
            var q = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var pattern = $"%{query.Trim()}%";
                q = q.Where(u =>
                    (u.UserName != null && EF.Functions.Like(u.UserName, pattern)) ||
                    (u.Email != null && EF.Functions.Like(u.Email, pattern)) ||
                    (u.FirstName != null && EF.Functions.Like(u.FirstName, pattern)) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName, pattern)) ||
                    (u.Phone != null && EF.Functions.Like(u.Phone, pattern)) ||
                    (u.Avatar != null && EF.Functions.Like(u.Avatar, pattern))
                );
            }

            if (roleName != null)
                q = q.Where(u => u.RoleName == roleName.Value);

            if (status != null)
                q = q.Where(u => u.Status == status.Value);

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var total = await q.CountAsync();
            var items = await q
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<User?> FindByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> FindById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> UpdateUser(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync();
        }
    }
}