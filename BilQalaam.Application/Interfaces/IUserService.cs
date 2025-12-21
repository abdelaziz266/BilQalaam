using BilQalaam.Application.DTOs.Users;

namespace BilQalaam.Application.Interfaces
{
    public interface IUserService
    {
        Task<(IEnumerable<UserResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<UserResponseDto?> GetByIdAsync(string id);
        Task<string> CreateAsync(CreateUserDto dto);
        Task<bool> UpdateAsync(string id, UpdateUserDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
