using BilQalaam.Application.DTOs.Lessons;

namespace BilQalaam.Application.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonResponseDto>> GetAllAsync();
        Task<LessonResponseDto?> GetByIdAsync(int id);

        // 🔍 فلترة الدروس
        Task<IEnumerable<LessonResponseDto>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<LessonResponseDto>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<LessonResponseDto>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<LessonResponseDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        Task<int> CreateAsync(CreateLessonDto dto, int teacherId, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateLessonDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
