using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface ISupervisorService
    {
        Task<(IEnumerable<SupervisorResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<SupervisorResponseDto?> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CreateSupervisorDto dto, string createdByUserId);
        Task<Result<bool>> UpdateAsync(int id, UpdateSupervisorDto dto, string updatedByUserId);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
