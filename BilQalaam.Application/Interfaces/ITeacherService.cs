using BilQalaam.Application.DTOs.Teachers;

namespace BilQalaam.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherResponseDto>> GetAllAsync();
        Task<TeacherResponseDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateTeacherDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateTeacherDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
