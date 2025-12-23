using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using System;
using System.Collections.Generic;

namespace BilQalaam.Application.Interfaces
{
    public interface ILessonService
    {
        Task<PaginatedResponseDto<LessonResponseDto>> GetAllAsync(
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

        Task<LessonResponseDto?> GetByIdAsync(int id);

        // 🔍 فلترة الدروس
        Task<(IEnumerable<LessonResponseDto>, int)> GetByTeacherIdAsync(int teacherId, int pageNumber = 1, int pageSize = 10);
        Task<(IEnumerable<LessonResponseDto>, int)> GetByTeacherIdsAsync(IEnumerable<int> teacherIds, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<LessonResponseDto>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<LessonResponseDto>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<LessonResponseDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        Task<int> CreateAsync(CreateLessonDto dto, int teacherId, string createdByUserId, int? supervisorId = null);
        Task<bool> UpdateAsync(int id, UpdateLessonDto dto, string updatedByUserId, int teacherId, int? supervisorId = null);
        Task<bool> DeleteAsync(int id);
    }
}
