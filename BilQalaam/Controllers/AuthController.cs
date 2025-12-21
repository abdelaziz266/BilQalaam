using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
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
        // التحقق من صحة البيانات
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(
                ApiResponseDto<LoginResponseDto>.Fail(
                    errors,
                    "فشل التحقق من البيانات",
                    400
                )
            );
        }

        try
        {
            var result = await _authService.LoginAsync(dto);

            return Ok(
                ApiResponseDto<LoginResponseDto>.Success(
                    result,
                    "تم تسجيل الدخول بنجاح",
                    200
                )
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponseDto<LoginResponseDto>.Fail(
                    new List<string> { ex.Message },
                    "غير مصرح",
                    401
                )
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponseDto<LoginResponseDto>.Fail(
                    new List<string> { ex.Message },
                    "حدث خطأ",
                    500
                )
            );
        }
    }
}
