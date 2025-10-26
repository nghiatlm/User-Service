using UserService.BO.DTO;
using System.Threading.Tasks;

namespace UserService.Service
{
    public interface IAuthService
    {
        Task<bool> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task SendEmailOtpAsync(string email);
        Task<bool> VerifyEmailOtp(string email, string code);
        Task SendPasswordResetAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
        bool VerifyPasswordResetToken(string email, string token);
    }
}