using BilQalaam.Application.DTOs.Students;

namespace BilQalaam.Application.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentResponseDto>> GetAllAsync();
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<StudentResponseDto>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<StudentResponseDto>> GetByTeacherIdAsync(int teacherId);
        Task<int> CreateAsync(CreateStudentDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
