using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface ILessonService
    {
        Task<Result<PaginatedResponseDto<LessonResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            IEnumerable<int>? supervisorIds,
            IEnumerable<int>? teacherIds,
            IEnumerable<int>? studentIds,
            IEnumerable<int>? familyIds,
            DateTime? fromDate,
            DateTime? toDate,
            string role,
            string userId);

        Task<Result<LessonResponseDto>> GetByIdAsync(int id, string role, string userId);

        Task<Result<int>> CreateAsync(CreateLessonDto dto, string role, string userId);

        Task<Result<bool>> UpdateAsync(int id, UpdateLessonDto dto, string userId);

        Task<Result<bool>> DeleteAsync(int id);
    }
}
