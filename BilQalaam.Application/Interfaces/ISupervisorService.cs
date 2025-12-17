using BilQalaam.Application.DTOs.Supervisors;

namespace BilQalaam.Application.Interfaces
{
    public interface ISupervisorService
    {
        Task<IEnumerable<SupervisorResponseDto>> GetAllAsync();
        Task<SupervisorResponseDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateSupervisorDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateSupervisorDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
