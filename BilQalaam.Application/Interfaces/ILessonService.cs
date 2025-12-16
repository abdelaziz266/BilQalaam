using BilQalaam.Application.DTOs.Lessons;

namespace BilQalaam.Application.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonResponseDto>> GetAllAsync();

        Task<LessonResponseDto?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateLessonDto dto);

        Task<bool> UpdateAsync(int id, UpdateLessonDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
