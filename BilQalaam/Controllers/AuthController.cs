using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BilQalaam.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<LoginResponseDto>.Fail(errors, "فشل التحقق من البيانات", 400));
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponseDto<LoginResponseDto>.Fail(
                    result.Errors,
                    "بيانات الدخول غير صحيحة",
                    401
                ));
            }

            return Ok(ApiResponseDto<LoginResponseDto>.Success(result.Data!, "تم تسجيل الدخول بنجاح", 200));
        }
    }
}
