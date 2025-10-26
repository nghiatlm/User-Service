using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.BO.DTO;
using UserService.Service;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);
            return Ok(ApiResponse<string>.SuccessResponse("Registration successful"));
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendEmailOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Email is required."));
            }

            await _authService.SendEmailOtpAsync(request.Email);

            return Ok(ApiResponse<object>.SuccessResponse(null, "OTP sent. If the email exists, check your inbox."));
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyEmailOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Email and OTP are required."));
            }
            var verified = await _authService.VerifyEmailOtp(request.Email, request.Otp);

            if (!verified)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid or expired OTP."));
            }
            return Ok(ApiResponse<object>.SuccessResponse(null, "OTP verified."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] SendPasswordResetRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Email là bắt buộc."));
            }
            await _authService.SendPasswordResetAsync(request.Email);
            return Ok((ApiResponse<object>.SuccessResponse(null, "Nếu email tồn tại, một liên kết đặt lại đã được gửi. Vui lòng kiểm tra hộp thư.")));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Email, token và mật khẩu mới là bắt buộc."));
            }

            await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đặt lại mật khẩu thành công."));
        }
    }
}