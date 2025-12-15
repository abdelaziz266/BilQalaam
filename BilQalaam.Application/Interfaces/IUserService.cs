using BilQalaam.Application.DTOs.Users;

namespace BilQalaam.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(string id);
        Task<string> CreateAsync(CreateUserDto dto);
        Task<bool> UpdateAsync(string id, UpdateUserDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
