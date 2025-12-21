namespace BilQalaam.Application.Results
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        // Success Result
        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Errors = new()
            };
        }

        // Failure Result
        public static Result<T> Failure(List<string> errors)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Errors = errors
            };
        }

        // Failure Result with single error
        public static Result<T> Failure(string error)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Errors = new List<string> { error }
            };
        }
    }
}
