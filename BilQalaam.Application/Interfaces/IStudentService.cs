using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IStudentService
    {
        Task<Result<PaginatedResponseDto<StudentResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            IEnumerable<int>? familyIds,
            IEnumerable<int>? teacherIds,
            string role,
            string userId,
            string? searchText = null);

        Task<Result<StudentResponseDto>> GetByIdAsync(int id, string role, string userId);

        Task<Result<int>> CreateAsync(CreateStudentDto dto, string role, string userId);

        Task<Result<IEnumerable<int>>> CreateMultipleAsync(CreateMultipleStudentsDto dto, string role, string userId);

        Task<Result<bool>> UpdateAsync(int id, UpdateStudentDto dto, string userId);

        Task<Result<bool>> DeleteAsync(int id);

        /// <summary>
        /// Get students by teacher ID
        /// </summary>
        Task<Result<PaginatedResponseDto<StudentResponseDto>>> GetByTeacherIdAsync(
            int teacherId);
    }
}
