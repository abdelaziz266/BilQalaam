using BilQalaam.Application.DTOs.Auth;
namespace BilQalaam.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    }

}
