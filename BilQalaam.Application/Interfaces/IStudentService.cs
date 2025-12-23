using BilQalaam.Application.DTOs.Students;

namespace BilQalaam.Application.Interfaces
{
    public interface IStudentService
    {
        Task<(IEnumerable<StudentResponseDto>, int)> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            IEnumerable<int>? familyIds = null,
            IEnumerable<int>? teacherIds = null);
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<StudentResponseDto>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<StudentResponseDto>> GetByTeacherIdAsync(int teacherId);
        Task<int> CreateAsync(CreateStudentDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<StudentResponseDto>, int)> GetByFamilyIdsAsync(
    IEnumerable<int> familyIds,
    int pageNumber,
    int pageSize);
    }
}
