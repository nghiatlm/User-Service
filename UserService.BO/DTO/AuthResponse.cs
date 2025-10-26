using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.BO.DTO
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public UserResponse UserResponse { get; set; }
    }
}