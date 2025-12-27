using BilQalaam.Application.DTOs.Auth;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto dto);
    }
}
