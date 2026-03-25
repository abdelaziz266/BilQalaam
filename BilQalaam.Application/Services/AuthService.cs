using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BilQalaam.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Result<LoginResponseDto>.Failure("بيانات الدخول غير صحيحة");

                // Check if user is deleted
                if (user.IsDeleted)
                    return Result<LoginResponseDto>.Failure("حساب المستخدم معطل");

                var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
                if (!validPassword)
                    return Result<LoginResponseDto>.Failure("بيانات الدخول غير صحيحة");

                var (token, expiresAt) = _tokenService.GenerateToken(user);

                return Result<LoginResponseDto>.Success(new LoginResponseDto
                {
                    Token = token,
                    Expiration = expiresAt,
                    UserId = user.Id,
                    Email = user.Email!,
                    Role = user.Role?.ToString() ?? ""
                });
            }
            catch (Exception ex)
            {
                return Result<LoginResponseDto>.Failure($"خطأ في تسجيل الدخول: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Result<bool>.Failure("المستخدم غير موجود");

                var validPassword = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
                if (!validPassword)
                    return Result<bool>.Failure("كلمة المرور الحالية غير صحيحة");

                var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<bool>.Failure(errors);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في تغيير كلمة المرور: {ex.Message}");
            }
        }
    }
}
