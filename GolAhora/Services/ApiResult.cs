using Microsoft.AspNetCore.Http;

namespace GolAhora.Services
{
    public class ApiResult<T>
    {
        private ApiResult(bool success, int statusCode, T? data, string? message)
        {
            Success = success;
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }

        public bool Success { get; }
        public int StatusCode { get; }
        public T? Data { get; }
        public string? Message { get; }

        public static ApiResult<T> Ok(T data) => new(true, StatusCodes.Status200OK, data, null);
        public static ApiResult<T> BadRequest(string message) => new(false, StatusCodes.Status400BadRequest, default, message);
        public static ApiResult<T> NotFound(string message = "Recurso inexistente.") => new(false, StatusCodes.Status404NotFound, default, message);
    }
}
