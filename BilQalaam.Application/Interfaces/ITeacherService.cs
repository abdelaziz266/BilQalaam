using BilQalaam.Application.DTOs.Teachers;

namespace BilQalaam.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<(IEnumerable<TeacherResponseDto>, int)> GetAllAsync(string currentUserId,string role, int pageNumber = 1, int pageSize = 10);
        Task<TeacherResponseDto?> GetByIdAsync(int id);
        Task<(IEnumerable<TeacherResponseDto>, int)> GetBySupervisorIdsAsync(
            IEnumerable<int> supervisorIds,
            int pageNumber,
            int pageSize);
        Task<int> CreateAsync(CreateTeacherDto dto, string createdByUserId);
        Task<bool> UpdateAsync(int id, UpdateTeacherDto dto, string updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}
