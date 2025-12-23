using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDto<LessonResponseDto>> GetAllAsync(
                        int pageNumber,
                        int pageSize,
                        IEnumerable<int>? supervisorIds,
                        IEnumerable<int>? teacherIds,
                        IEnumerable<int>? studentIds,
                        IEnumerable<int>? familyIds,
                        DateTime? fromDate,
                        DateTime? toDate,
                        string role,
                        string userId)
        {
            IQueryable<Lesson> query = _unitOfWork
                .Repository<Lesson>()
                .Query()
                .Include(l => l.Student)
                .Include(l => l.Teacher)
                .Include(l => l.Supervisor)
                .Include(l => l.Family); 

            if (supervisorIds?.Any() == true)
                query = query.Where(l => l.SupervisorId.HasValue && supervisorIds.Contains(l.SupervisorId.Value));

            if (teacherIds?.Any() == true)
                query = query.Where(l => teacherIds.Contains(l.TeacherId));

            if (studentIds?.Any() == true)
                query = query.Where(l => studentIds.Contains(l.StudentId));

            if (familyIds?.Any() == true)
                query = query.Where(l => familyIds.Contains(l.FamilyId));

            if (fromDate.HasValue)
                query = query.Where(l => l.LessonDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.LessonDate <= toDate.Value);

            // Role logic هنا
            query = ApplyRoleFilter(query, role, userId);

            var totalCount = await query.CountAsync();

            var lessons = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDto<LessonResponseDto>
            {
                Items = _mapper.Map<List<LessonResponseDto>>(lessons),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                PagesCount = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<LessonResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var lesson = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .FirstOrDefaultAsync(l => l.Id == id);

                return lesson == null ? null : _mapper.Map<LessonResponseDto>(lesson);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب الدرس: {ex.Message}" });
            }
        }

        public async Task<(IEnumerable<LessonResponseDto>, int)> GetByTeacherIdAsync(int teacherId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .Where(l => l.TeacherId == teacherId)
                    .ToListAsync();

                var totalCount = lessons.Count();
                var paginatedLessons = lessons
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<LessonResponseDto>>(paginatedLessons), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب دروس المعلم: {ex.Message}" });
            }
        }

        public async Task<(IEnumerable<LessonResponseDto>, int)> GetByTeacherIdsAsync(IEnumerable<int> teacherIds, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var teacherIdSet = new HashSet<int>(teacherIds ?? Enumerable.Empty<int>());
                if (!teacherIdSet.Any())
                    return (Enumerable.Empty<LessonResponseDto>(), 0);

                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .Where(l => teacherIdSet.Contains(l.TeacherId))
                    .ToListAsync();

                var totalCount = lessons.Count();
                var paginatedLessons = lessons
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<LessonResponseDto>>(paginatedLessons), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب دروس المعلمين: {ex.Message}" });
            }
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByFamilyIdAsync(int familyId)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .Where(l => l.FamilyId == familyId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب دروس العائلة: {ex.Message}" });
            }
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByStudentIdAsync(int studentId)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .Where(l => l.StudentId == studentId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب دروس الطالب: {ex.Message}" });
            }
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family)
                    .Where(l => l.LessonDate >= fromDate && l.LessonDate <= toDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب الدروس حسب التاريخ: {ex.Message}" });
            }
        }

        public async Task<int> CreateAsync(CreateLessonDto dto, int teacherId, string createdByUserId, int? supervisorId = null)
        {
            try
            {
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);
                if (student == null)
                    throw new ValidationException(new List<string> { "الطالب غير موجود" });

                if (student.TeacherId != teacherId)
                    throw new ValidationException(new List<string> { "هذا الطالب ليس من طلابك" });

                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                if (teacher == null)
                    throw new ValidationException(new List<string> { "المعلم غير موجود" });

                var lesson = new Lesson
                {
                    StudentId = dto.StudentId,
                    TeacherId = teacherId,
                    SupervisorId = supervisorId,
                    FamilyId = student.FamilyId??0,
                    LessonDate = dto.LessonDate,
                    DurationMinutes = dto.DurationMinutes,
                    Notes = dto.Notes,
                    Evaluation = dto.Evaluation,
                    StudentHourlyRate = student.HourlyRate,
                    TeacherHourlyRate = teacher.HourlyRate,
                    Currency = student.Currency,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdByUserId
                };

                await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
                await _unitOfWork.CompleteAsync();

                return lesson.Id;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في إنشاء الدرس: {ex.Message}" });
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateLessonDto dto, string updatedByUserId, int teacherId, int? supervisorId = null)
        {
            try
            {
                var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
                if (lesson == null) return false;

                var studentIdToUse = dto.StudentId ?? lesson.StudentId;
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(studentIdToUse);
                if (student == null)
                    throw new ValidationException(new List<string> { "الطالب غير موجود" });

                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                if (teacher == null)
                    throw new ValidationException(new List<string> { "المعلم غير موجود" });

                lesson.StudentId = student.Id;
                lesson.FamilyId = student.FamilyId ?? 0;
                lesson.StudentHourlyRate = student.HourlyRate;
                lesson.Currency = student.Currency;

                lesson.TeacherId = teacherId;
                lesson.TeacherHourlyRate = teacher.HourlyRate;
                lesson.SupervisorId = supervisorId;

                if (dto.LessonDate.HasValue)
                    lesson.LessonDate = dto.LessonDate.Value;

                if (dto.DurationMinutes.HasValue)
                    lesson.DurationMinutes = dto.DurationMinutes.Value;

                if (dto.Notes != null)
                    lesson.Notes = dto.Notes;

                if (dto.Evaluation.HasValue)
                    lesson.Evaluation = dto.Evaluation.Value;

                lesson.UpdatedAt = DateTime.UtcNow;
                lesson.UpdatedBy = updatedByUserId;

                _unitOfWork.Repository<Lesson>().Update(lesson);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في تحديث الدرس: {ex.Message}" });
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
                if (lesson == null) return false;

                // 🗑️ Soft Delete
                lesson.IsDeleted = true;
                lesson.DeletedAt = DateTime.UtcNow;
                lesson.DeletedBy = id.ToString();

                _unitOfWork.Repository<Lesson>().Update(lesson);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في حذف الدرس: {ex.Message}" });
            }
        }







        private IQueryable<Lesson> ApplyRoleFilter(
    IQueryable<Lesson> query,
    string role,
    string userId)
        {
            if (role == "Teacher")
                return query.Where(l => l.Teacher.UserId == userId);

            if (role == "Admin")
                return query.Where(l => l.Teacher.Supervisor.UserId == userId);

            return query; // SuperAdmin
        }

    }
}
