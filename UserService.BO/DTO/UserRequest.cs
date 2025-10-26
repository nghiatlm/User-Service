
using UserService.BO.Enums;

namespace UserService.BO.DTO
{
    public class UserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public RoleName RoleName { get; set; }

    }
}