using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using UserService.BO.Exceptions;
using UserService.BO.Entities;
using UserService.BO.DTO;
using UserService.BO.Enums;
using UserService.Repository;

namespace UserService.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan PasswordResetTtl = TimeSpan.FromMinutes(30);

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            IMemoryCache cache,
            IEmailService emailService,
            IConfiguration configuration
        )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _cache = cache;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.FindByEmail(request.Email);
            if (user == null) throw new AppException("User not found", HttpStatusCode.NotFound);
            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Password);
            if (!isPasswordValid) throw new AppException("Invalid password", HttpStatusCode.BadRequest);
            string token = _jwtService.GenerateJwtToken(user);
            if (token == null)
                throw new AppException("Token cannot be null", HttpStatusCode.BadRequest);
            return new AuthResponse
            {
                Token = token,
                UserResponse = new UserResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleName = user.RoleName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var user = await _userRepository.FindByEmail(request.Email);
            if (user != null) throw new AppException("User already exit", HttpStatusCode.BadRequest);
            var result = await _userRepository.AddUser(new User
            {
                Email = request.Email,
                Password = _passwordHasher.HashPassword(request.Password),
                RoleName = RoleName.ROLE_USER,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = Status.INACTIVE
            });
            return result > 0 ? true : false;
        }

        public async Task SendEmailOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("Email is required.", HttpStatusCode.BadRequest);

            var rng = new Random();
            var otp = rng.Next(100000, 999999).ToString("D6");
            var cacheKey = $"email_otp_{email}";
            _cache.Set(cacheKey, otp, OtpTtl);

            var subject = "Xác thực tài khoản — Mã OTP của bạn";
            var body = $@"
    <div style='font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;padding:24px;color:#222;background:#ffffff;'>
      <div style='text-align:center;padding-bottom:12px;'>
        <h1 style='margin:0;font-size:20px;color:#0d6efd;'>User Service</h1>
        <p style='margin:6px 0 0;color:#6c757d;font-size:13px;'>Yêu cầu xác thực email</p>
      </div>

      <div style='margin-top:18px;padding:20px;border-radius:8px;background:#f8f9fa;text-align:center;'>
        <p style='margin:0;color:#333;font-size:14px;'>Mã xác thực (OTP) của bạn:</p>
        <div style='display:inline-block;margin:14px 0;padding:12px 20px;border-radius:6px;background:#fff;border:1px solid #e9ecef;'>
          <span style='font-size:36px;font-weight:700;letter-spacing:4px;color:#111;'>{otp}</span>
        </div>
        <p style='margin:0;color:#6c757d;font-size:13px;'>Mã sẽ tự hết hạn sau <strong>{OtpTtl.TotalMinutes} phút</strong>.</p>
      </div>

      <div style='margin-top:18px;font-size:13px;color:#495057;line-height:1.4;'>
        <p style='margin:0;'>Nếu bạn không yêu cầu mã này, bạn có thể bỏ qua email này. Để bảo mật, không chia sẻ mã với bất kỳ ai.</p>
        <p style='margin:10px 0 0;color:#6c757d;font-size:12px;'>Trân trọng,<br/>User Service Team</p>
      </div>

      <div style='margin-top:20px;font-size:11px;color:#adb5bd;text-align:center;'>
        © {DateTime.UtcNow.Year} User Service. Mọi quyền được bảo lưu.
      </div>
    </div>";

            try
            {
                await _emailService.SendAsync(email, subject, body);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new AppException("Không thể gửi email OTP. Vui lòng thử lại sau.", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<bool> VerifyEmailOtp(string email, string code)
        {
            var cacheKey = $"email_otp_{email}";
            if (_cache.TryGetValue<string>(cacheKey, out var expected))
            {
                if (expected == code)
                {
                    _cache.Remove(cacheKey);
                    var user = await _userRepository.FindByEmail(email);
                    if (user != null && user.Status == Status.INACTIVE)
                    {
                        user.Status = Status.ACTIVE;
                        await _userRepository.UpdateUser(user);
                    }
                    return true;
                }
            }
            return false;
        }

        private static string GenerateSecureToken(int bytes = 32)
        {
            var data = new byte[bytes];
            RandomNumberGenerator.Fill(data);
            var token = Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return token;
        }

        public async Task SendPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("Email is required.", HttpStatusCode.BadRequest);

            var token = GenerateSecureToken();
            var cacheKey = $"pwd_reset_{email}";
            _cache.Set(cacheKey, token, PasswordResetTtl);

            // frontend base: allow ENV override like JwtService
            var frontendBase = Environment.GetEnvironmentVariable("FRONTEND_BASEURL")
                              ?? _configuration["Frontend:BaseUrl"]
                              ?? _configuration["App:FrontendBaseUrl"]
                              ?? string.Empty;

            string resetUrl;
            if (!string.IsNullOrWhiteSpace(frontendBase))
            {
                resetUrl = $"{frontendBase.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            }
            else
            {
                resetUrl = $"token:{token}";
            }

            var subject = "Yêu cầu đặt lại mật khẩu";
            var body = $@"
<div style='font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;padding:24px;color:#222;background:#ffffff;'>
  <div style='text-align:center;padding-bottom:12px;'>
    <h1 style='margin:0;font-size:20px;color:#0d6efd;'>User Service</h1>
    <p style='margin:6px 0 0;color:#6c757d;font-size:13px;'>Yêu cầu đặt lại mật khẩu</p>
  </div>

  <div style='margin-top:18px;padding:20px;border-radius:8px;background:#f8f9fa;text-align:center;'>
    <p style='margin:0;color:#333;font-size:14px;'>Bạn nhận được email này vì có yêu cầu đặt lại mật khẩu cho tài khoản liên kết với <strong>{email}</strong>.</p>
    <div style='margin:18px 0;'>
      <a href='{resetUrl}' style='display:inline-block;padding:12px 20px;border-radius:6px;background:#0d6efd;color:#fff;text-decoration:none;font-weight:600;'>Đặt lại mật khẩu</a>
    </div>
    <p style='margin:0;color:#6c757d;font-size:13px;'>Liên kết sẽ hết hạn sau <strong>{PasswordResetTtl.TotalMinutes} phút</strong>.</p>
  </div>

  <div style='margin-top:18px;font-size:13px;color:#495057;line-height:1.4;'>
    <p style='margin:0;'>Nếu bạn không yêu cầu đặt lại mật khẩu, bạn có thể bỏ qua email này.</p>
    <p style='margin:10px 0 0;color:#6c757d;font-size:12px;'>Trân trọng,<br/>User Service Team</p>
  </div>

  <div style='margin-top:20px;font-size:11px;color:#adb5bd;text-align:center;'>
    © {DateTime.UtcNow.Year} User Service. Mọi quyền được bảo lưu.
  </div>
</div>";

            try
            {
                await _emailService.SendAsync(email, subject, body);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new AppException("Không thể gửi email đặt lại mật khẩu. Vui lòng thử lại sau.", HttpStatusCode.InternalServerError);
            }
        }

        public bool VerifyPasswordResetToken(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return false;

            var cacheKey = $"pwd_reset_{email}";
            if (_cache.TryGetValue<string>(cacheKey, out var expected) && expected == token)
            {
                return true;
            }
            return false;
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                throw new AppException("Email, token và mật khẩu mới là bắt buộc.", HttpStatusCode.BadRequest);

            var cacheKey = $"pwd_reset_{email}";
            if (!_cache.TryGetValue<string>(cacheKey, out var expected) || expected != token)
                throw new AppException("Token không hợp lệ hoặc đã hết hạn.", HttpStatusCode.BadRequest);
            _cache.Remove(cacheKey);
            var user = await _userRepository.FindByEmail(email);
            if (user == null)
                throw new AppException("Người dùng không tồn tại.", HttpStatusCode.NotFound);

            user.Password = _passwordHasher.HashPassword(newPassword);
            await _userRepository.UpdateUser(user);
        }
    }
}