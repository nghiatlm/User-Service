
using System.Net;

namespace UserService.BO.DTO
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public ApiResponse(HttpStatusCode statusCode, bool success, string? message = default, T? data = default)
        {
            StatusCode = (int)statusCode;
            Success = success;
            Message = message;
            Data = data;
        }

        public ApiResponse(HttpStatusCode statusCode, bool success, string message)
        {
            this.StatusCode = (int)statusCode;
            Success = success;
            Message = message;
        }

        public static ApiResponse<T> SuccessResponse(T? data = default, string message = "thành công")
        {
            return new ApiResponse<T>(HttpStatusCode.OK, true, message, data);
        }

        public static ApiResponse<T> CreatedSuccess(T? data = default, string message = "tạo thành công")
        {
            return new ApiResponse<T>(HttpStatusCode.Created, true, message, data);
        }

        public static ApiResponse<T> BadRequest(string message = "Lỗi")
        {
            return new ApiResponse<T>(HttpStatusCode.BadRequest, false, message);
        }

        public static ApiResponse<T> DeleteSuccess(string message = "Xóa thành công")
        {
            return new ApiResponse<T>(HttpStatusCode.NoContent, true, message);
        }

        public static ApiResponse<T> Unauthorized(string message = "Token không hợp lệ hoặc đã hết hạn")
        {
            return new ApiResponse<T>(HttpStatusCode.Unauthorized, false, message);
        }

        public static ApiResponse<T> Forbidden(string message = "Không có quyền truy cập")
        {
            return new ApiResponse<T>(HttpStatusCode.Forbidden, false, message);
        }

        public static ApiResponse<T> NotFound(string message = "không tìm thấy")
        {
            return new ApiResponse<T>(HttpStatusCode.NotFound, false, message);
        }

        public static ApiResponse<T> ServerError(string message = "Internal Server Error")
        {
            return new ApiResponse<T>(HttpStatusCode.InternalServerError, false, message);
        }
    }
}