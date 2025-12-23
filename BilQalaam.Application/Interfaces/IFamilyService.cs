using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<(IEnumerable<FamilyResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<FamilyResponseDto?> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CreateFamilyDto dto, string createdByUserId);
        Task<Result<bool>> UpdateAsync(int id, UpdateFamilyDto dto, string updatedByUserId);
        Task<Result<bool>> DeleteAsync(int id);
        Task<(IEnumerable<FamilyResponseDto>, int)> GetBySupervisorIdsAsync(
    IEnumerable<int> supervisorIds,
    int pageNumber,
    int pageSize);
    }
}
