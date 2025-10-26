using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.BO.DTO;

namespace UserService.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;
        private readonly JwtSettings _jwtSettings;

        private static readonly string[] _excludedPaths = new[]
        {
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/send-otp",
            "/api/v1/auth/verify-otp",
            "/swagger/index.html"
        };

        public JwtMiddleware(
            RequestDelegate next,
            ILogger<JwtMiddleware> logger,
            IOptions<JwtSettings> jwtOptions
        )
        {
            _next = next;
            _logger = logger;
            _jwtSettings = jwtOptions.Value;
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

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            _logger.LogInformation($"Processing request for path: {path}");

            if (_excludedPaths.Any(p => path.StartsWith(p)))
            {
                _logger.LogInformation($"Skipping JWT validation for public path: {path}");
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            string token = null;

            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = authHeader.Substring("Bearer ".Length).Trim();
                else
                    token = authHeader.Trim();
            }

            if (token != null)
            {
                try
                {
                    _logger.LogInformation("JWT token found in request");
                    AttachUserToContext(context, token);
                }
                catch (SecurityTokenExpiredException)
                {
                    await WriteErrorResponse(
                        context,
                        HttpStatusCode.Unauthorized,
                        "Token has expired"
                    );
                    return;
                }
                catch (SecurityTokenException ex)
                {
                    await WriteErrorResponse(
                        context,
                        HttpStatusCode.Unauthorized,
                        $"Invalid token: {ex.Message}"
                    );
                    return;
                }
                catch (Exception ex)
                {
                    await WriteErrorResponse(
                        context,
                        HttpStatusCode.InternalServerError,
                        $"Unexpected error: {ex.Message}"
                    );
                    return;
                }
            }
            else
            {
                await WriteErrorResponse(
                    context,
                    HttpStatusCode.Unauthorized,
                    "Missing or invalid Authorization header"
                );
                return;
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                RoleClaimType = ClaimTypes.Role,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = false,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken
            );

            if (validatedToken is not JwtSecurityToken)
                throw new SecurityTokenException("Invalid JWT token format");

            context.User = principal;
        }

        private async Task WriteErrorResponse(
            HttpContext context,
            HttpStatusCode statusCode,
            string message
        )
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>(
                statusCode: statusCode,
                success: false,
                message: message
            );

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}