
using System.Security.Claims;
using UserService.BO.Entities;

namespace UserService.Service
{
    public interface IJwtService
    {
        public string GenerateJwtToken(User _user);
        public int? ValidateToken(string token);
        public ClaimsPrincipal ValidateTokenClaimsPrincipal(string token);
        public string RefeshToken(string email);
    }
}