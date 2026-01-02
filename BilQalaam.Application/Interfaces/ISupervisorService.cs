using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface ISupervisorService
    {
        Task<Result<PaginatedResponseDto<SupervisorResponseDto>>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchText = null);

        Task<SupervisorResponseDto?> GetByIdAsync(int id);

        Task<Result<int>> CreateAsync(CreateSupervisorDto dto, string createdByUserId);

        Task<Result<bool>> UpdateAsync(int id, UpdateSupervisorDto dto, string updatedByUserId);

        Task<Result<bool>> DeleteAsync(int id);

        /// <summary>
        /// Get supervisor details with related teachers, families, and students
        /// </summary>
        Task<Result<SupervisorDetailsDto>> GetSupervisorDetailsAsync(int supervisorId);

        /// <summary>
        /// Get multiple supervisors details with related teachers, families, and students
        /// </summary>
        Task<Result<List<SupervisorDetailsDto>>> GetMultipleSupervisorsDetailsAsync(IEnumerable<int> supervisorIds);
    }
}
