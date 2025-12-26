using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<Result<PaginatedResponseDto<FamilyResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string role,
            string userId);

        Task<Result<FamilyResponseDto>> GetByIdAsync(int id, string role, string userId);

        Task<Result<int>> CreateAsync(CreateFamilyDto dto, string role, string userId);

        Task<Result<bool>> UpdateAsync(int id, UpdateFamilyDto dto, string userId);

        Task<Result<bool>> DeleteAsync(int id);

        /// <summary>
        /// Get families by teacher ID (same supervisor)
        /// </summary>
        Task<Result<PaginatedResponseDto<FamilyResponseDto>>> GetByTeacherIdAsync(
            int teacherId,
            int pageNumber,
            int pageSize);
    }
}
