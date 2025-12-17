using BilQalaam.Application.DTOs.Families;

namespace BilQalaam.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<IEnumerable<FamilyResponseDto>> GetAllAsync();
        Task<FamilyResponseDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateFamilyDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateFamilyDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
