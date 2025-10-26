using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UserService.BO.Enums;

namespace UserService.BO.DTO
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;

        [EnumDataType(typeof(RoleName))]
        public RoleName RoleName { get; set; }

        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}