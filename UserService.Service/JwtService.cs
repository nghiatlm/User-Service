
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.BO.DTO;
using UserService.BO.Entities;

namespace UserService.Service
{
    public class JwtService : IJwtService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;

        public JwtService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<JwtSettings> jwtOptions
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtOptions.Value;
            // Ghi đè từ biến môi trường nếu tồn tại
            _jwtSettings.SecretKey =
                Environment.GetEnvironmentVariable("JWT_KEY") ?? _jwtSettings.SecretKey;
            _jwtSettings.Issuer =
                Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _jwtSettings.Issuer;
            _jwtSettings.Audience =
                Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _jwtSettings.Audience;

            string? envExpires = Environment.GetEnvironmentVariable("JWT_EXPIRES_IN_MINUTES");
            if (int.TryParse(envExpires, out int expiresInMinutes))
            {
                _jwtSettings.ExpiresInMinutes = expiresInMinutes;
            }
        }

        public string GenerateJwtToken(User _user)
        {
            var secretKey = _jwtSettings.SecretKey;
            var issuer = _jwtSettings.Issuer;
            var audience = _jwtSettings.Audience;
            var expiryMinutes = _jwtSettings.ExpiresInMinutes.ToString();

            if (
                string.IsNullOrEmpty(secretKey)
                || string.IsNullOrEmpty(issuer)
                || string.IsNullOrEmpty(audience)
                || string.IsNullOrEmpty(expiryMinutes)
            )
            {
                throw new InvalidOperationException(
                    "JWT environment variables are not set properly."
                );
            }
            var key = Encoding.UTF8.GetBytes(secretKey);
            var claims = new List<Claim>
            {
                new Claim("id", _user.Id.ToString()),
                // new Claim("email", _account.Email),
                new Claim("email", _user.Email),
                new Claim(ClaimTypes.Role, _user.RoleName.ToString()),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(expiryMinutes)), // token hết hạng trong 30 phút
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public Guid GetAccountIdGuid()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            // thường claim NameIdentifier hoặc "sub"
            var raw =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("accountId")?.Value
                ?? throw new UnauthorizedAccessException(
                    "Claim 'accountId' hoặc NameIdentifier không tồn tại"
                );

            if (!Guid.TryParse(raw, out var accountId))
                throw new UnauthorizedAccessException(
                    "AccountId trong token không đúng định dạng GUID"
                );

            return accountId;
        }

        public string RefeshToken(string email)
        {
            throw new NotImplementedException();
        }

        public int? ValidateToken(string token)
        {
            if (token == null)
                return null;
            var secretKey = _jwtSettings.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT environment variables are not set properly."
                );
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            try
            {
                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero,
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                return userId;
            }
            catch
            {
                return null;
            }
        }

        public ClaimsPrincipal ValidateTokenClaimsPrincipal(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            return principal;
        }
    }
}