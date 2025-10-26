using UserService.BO.Enums;

namespace UserService.BO.DTO
{
    public class UserUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public Status? Status { get; set; }
    }
}