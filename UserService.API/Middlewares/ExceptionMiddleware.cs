
using System.Net;
using System.Text.Json;
using UserService.BO.DTO;
using UserService.BO.Exceptions;

namespace UserService.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException aex)
            {
                _logger.LogWarning(aex, "AppException");
                await WriteResponse(context, aex.StatusCode, aex.Message, aex.Details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteResponse(context, HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode status, string message, object? details = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var payload = ApiResponse<object>.ServerError(message);
            // If it's not server error, build custom payload
            if (status == HttpStatusCode.BadRequest) payload = ApiResponse<object>.BadRequest(message);
            if (status == HttpStatusCode.Unauthorized) payload = ApiResponse<object>.Unauthorized(message);
            if (status == HttpStatusCode.Forbidden) payload = ApiResponse<object>.Forbidden(message);
            if (status == HttpStatusCode.NotFound) payload = ApiResponse<object>.NotFound(message);
            if (status == HttpStatusCode.NoContent) payload = new ApiResponse<object>(status, true, message, details);

            // attach details if any
            if (details != null) payload.Data = details;

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAppExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}