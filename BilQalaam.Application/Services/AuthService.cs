using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
    }
}
