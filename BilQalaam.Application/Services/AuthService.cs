using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.Interfaces;
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

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
                throw new UnauthorizedAccessException("Invalid email or password");

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Expiration = expiresAt,
                UserId = user.Id,
                Email = user.Email!,
                Role = user.Role?.ToString() ?? ""
            };

        }
    }
}
