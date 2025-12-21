using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.Exceptions;
using System.Text.Json;

namespace BilQalaam.Api
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ÕœÀ Œÿ√ €Ì— „ Êﬁ⁄");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponseDto<object>();

            if (exception is ValidationException validationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ApiResponseDto<object>
                {
                    Status = 400,
                    Message = "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ",
                    Errors = validationException.Errors
                };
            }
            else if (exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = new ApiResponseDto<object>
                {
                    Status = 401,
                    Message = "€Ì— „’—Õ",
                    Errors = new List<string> { exception.Message }
                };
            }
            else if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new ApiResponseDto<object>
                {
                    Status = 404,
                    Message = "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    Errors = new List<string> { exception.Message }
                };
            }
            else if (exception is ArgumentException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ApiResponseDto<object>
                {
                    Status = 400,
                    Message = "»Ì«‰«  €Ì— ’ÕÌÕ…",
                    Errors = new List<string> { exception.Message }
                };
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new ApiResponseDto<object>
                {
                    Status = 500,
                    Message = "Œÿ√ ›Ì «·Œ«œ„",
                    Errors = new List<string> { exception.Message }
                };
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
