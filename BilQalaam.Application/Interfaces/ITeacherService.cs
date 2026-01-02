using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<Result<PaginatedResponseDto<TeacherResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string role,
            string userId,
            string? searchText = null);

        Task<Result<TeacherResponseDto>> GetByIdAsync(int id, string role, string userId);

        Task<Result<int>> CreateAsync(CreateTeacherDto dto, string role, string userId);

        Task<Result<bool>> UpdateAsync(int id, UpdateTeacherDto dto, string userId);

        Task<Result<bool>> DeleteAsync(int id);

        /// <summary>
        /// Get teachers by family ID (same supervisor)
        /// </summary>
        Task<Result<PaginatedResponseDto<TeacherResponseDto>>> GetByFamilyIdAsync(
            int familyId,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Get teacher details with related families and students
        /// </summary>
        Task<Result<TeacherDetailsDto>> GetTeacherDetailsAsync(int teacherId);

        /// <summary>
        /// Get multiple teachers details with related families and students
        /// </summary>
        Task<Result<List<TeacherDetailsDto>>> GetMultipleTeachersDetailsAsync(IEnumerable<int> teacherIds);
    }
}
