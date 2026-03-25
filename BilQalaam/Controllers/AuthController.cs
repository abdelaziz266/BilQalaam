using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

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

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<bool>.Fail(errors, "فشل التحقق من البيانات", 400));
            }

            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponseDto<bool>.Fail(
                    new List<string> { "لم يتم العثور على معرف المستخدم" },
                    "غير مصرح",
                    401
                ));
            }

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseDto<bool>.Fail(
                    result.Errors,
                    "فشل تغيير كلمة المرور",
                    400
                ));
            }

            return Ok(ApiResponseDto<bool>.Success(true, "تم تغيير كلمة المرور بنجاح", 200));
        }
    }
}
