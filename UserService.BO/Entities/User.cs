
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.BO.Enums;

namespace UserService.BO.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_name", TypeName = "nvarchar(100)")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Column("email", TypeName = "nvarchar(255)")]
        public string Email { get; set; }

        [Column("first_name", TypeName = "nvarchar(100)")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name", TypeName = "nvarchar(100)")]
        public string LastName { get; set; } = string.Empty;

        [Column("date_of_birth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("phone", TypeName = "nvarchar(15)")]
        public string Phone { get; set; } = string.Empty;

        [Column("password", TypeName = "nvarchar(255)")]
        public string Password { get; set; } = string.Empty;

        [Column("avatar", TypeName = "nvarchar(255)")]
        public string Avatar { get; set; } = string.Empty;

        [Required]
        [Column("role_name", TypeName = "nvarchar(50)")]
        [EnumDataType(typeof(RoleName))]
        public RoleName RoleName { get; set; }

        [Required]
        [Column("status", TypeName = "nvarchar(50)")]
        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
    }
}