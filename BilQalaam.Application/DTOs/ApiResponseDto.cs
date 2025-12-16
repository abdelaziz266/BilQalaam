namespace BilQalaam.Application.DTOs.Common
{
    public class ApiResponseDto<T>
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        // Success Response
        public static ApiResponseDto<T> Success(
            T data,
            string message = "Success",
            int status = 200)
        {
            return new ApiResponseDto<T>
            {
                Status = status,
                Message = message,
                Data = data
            };
        }

        // Error Response
        public static ApiResponseDto<T> Fail(
            List<string> errors,
            string message = "Failed",
            int status = 400)
        {
            return new ApiResponseDto<T>
            {
                Status = status,
                Message = message,
                Errors = errors
            };
        }
    }
}
