using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
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

        public async Task<Result<PaginatedResponseDto<LessonResponseDto>>> GetAllAsync(
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
            try
            {
                IQueryable<Lesson> query = _unitOfWork
                    .Repository<Lesson>()
                    .Query()
                    .Include(l => l.Student)
                    .Include(l => l.Teacher)
                    .Include(l => l.Supervisor)
                    .Include(l => l.Family);

                // Apply filters
                if (supervisorIds?.Any() == true)
                    query = query.Where(l => l.SupervisorId.HasValue && supervisorIds.Contains(l.SupervisorId.Value));

                // لو اختار معلمين - جيب الدروس والعائلات والطلاب المرتبطين بهم
                if (teacherIds?.Any() == true)
                {
                    var teacherIdSet = new HashSet<int>(teacherIds);
                    query = query.Where(l => teacherIdSet.Contains(l.TeacherId));
                }
                // لو اختار عائلات - جيب الدروس والمعلمين والطلاب المرتبطين بالعائلات
                else if (familyIds?.Any() == true)
                {
                    var familyIdSet = new HashSet<int>(familyIds);
                    query = query.Where(l => familyIdSet.Contains(l.FamilyId));
                }
                else if (studentIds?.Any() == true)
                    query = query.Where(l => studentIds.Contains(l.StudentId));

                if (fromDate.HasValue)
                    query = query.Where(l => l.LessonDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.LessonDate <= toDate.Value);

                // Apply role-based filter
                query = ApplyRoleFilter(query, role, userId);

                var totalCount = await query.CountAsync();
                var lessons = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<LessonResponseDto>>.Success(new PaginatedResponseDto<LessonResponseDto>
                {
                    Items = _mapper.Map<List<LessonResponseDto>>(lessons),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<LessonResponseDto>>.Failure($"خطأ في جلب الدروس: {ex.Message}");
            }
        }

        public async Task<Result<LessonResponseDto>> GetByIdAsync(int id, string role, string userId)
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

                if (lesson == null)
                    return Result<LessonResponseDto>.Failure("الدرس غير موجود");

                // Role-based access check
                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null || lesson.TeacherId != teacher.Id)
                        return Result<LessonResponseDto>.Failure("الدرس غير موجود");
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null || lesson.SupervisorId != supervisor.Id)
                        return Result<LessonResponseDto>.Failure("الدرس غير موجود");
                }

                return Result<LessonResponseDto>.Success(_mapper.Map<LessonResponseDto>(lesson));
            }
            catch (Exception ex)
            {
                return Result<LessonResponseDto>.Failure($"خطأ في جلب الدرس: {ex.Message}");
            }
        }

        public async Task<Result<int>> CreateAsync(CreateLessonDto dto, string role, string userId)
        {
            try
            {
                // Validate student
                var student = await _unitOfWork.Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .FirstOrDefaultAsync(s => s.Id == dto.StudentId);
                
                if (student == null)
                    return Result<int>.Failure("الطالب غير موجود");

                int teacherId;
                int? supervisorId = null;

                if (role == "Teacher")
                {
                    // Teacher creates lesson for their own students
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<int>.Failure("لم يتم العثور على بيانات المعلم");

                    if (student.TeacherId != teacher.Id)
                        return Result<int>.Failure("هذا الطالب ليس من طلابك");

                    teacherId = teacher.Id;
                    supervisorId = teacher.SupervisorId;
                }
                else if (role == "Admin")
                {
                    // Admin must provide TeacherId
                    if (!dto.TeacherId.HasValue)
                        return Result<int>.Failure("المعلم مطلوب");

                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId.Value);
                    if (teacher == null)
                        return Result<int>.Failure("المعلم غير موجود");

                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<int>.Failure("لم يتم العثور على بيانات المشرف");

                    // Verify teacher belongs to this supervisor
                    if (teacher.SupervisorId != supervisor.Id)
                        return Result<int>.Failure("هذا المعلم ليس تابعاً لك");

                    teacherId = teacher.Id;
                    supervisorId = supervisor.Id;
                }
                else // SuperAdmin
                {
                    if (!dto.TeacherId.HasValue)
                        return Result<int>.Failure("المعلم مطلوب");

                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId.Value);
                    if (teacher == null)
                        return Result<int>.Failure("المعلم غير موجود");

                    teacherId = teacher.Id;
                    supervisorId = teacher.SupervisorId;
                }

                var teacherEntity = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);

                // استخدام سعر العائلة فقط
                decimal studentHourlyRate = student.Family != null ? student.Family.HourlyRate : 0;
                var currency = student.Family != null ? student.Family.Currency : 0;

                var lesson = new Lesson
                {
                    StudentId = dto.StudentId,
                    TeacherId = teacherId,
                    SupervisorId = supervisorId,
                    FamilyId = student.FamilyId ?? 0,
                    LessonDate = dto.LessonDate,
                    DurationMinutes = dto.DurationMinutes,
                    Notes = dto.Notes,
                    IsAbsent = dto.IsAbsent,
                    Evaluation = dto.Evaluation,
                    StudentHourlyRate = studentHourlyRate,
                    TeacherHourlyRate = teacherEntity!.HourlyRate,
                    Currency = currency,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
                await _unitOfWork.CompleteAsync();

                return Result<int>.Success(lesson.Id);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"خطأ في إنشاء الدرس: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateLessonDto dto, string userId)
        {
            try
            {
                var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
                if (lesson == null)
                    return Result<bool>.Failure("الدرس غير موجود");

                var studentIdToUse = dto.StudentId ?? lesson.StudentId;
                var student = await _unitOfWork.Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .FirstOrDefaultAsync(s => s.Id == studentIdToUse);
                
                if (student == null)
                    return Result<bool>.Failure("الطالب غير موجود");

                // Update lesson fields
                lesson.StudentId = student.Id;
                lesson.FamilyId = student.FamilyId ?? 0;
                
                // استخدام سعر العائلة فقط
                lesson.StudentHourlyRate = student.Family != null ? student.Family.HourlyRate : 0;
                lesson.Currency = student.Family != null ? student.Family.Currency : 0;

                if (dto.LessonDate.HasValue)
                    lesson.LessonDate = dto.LessonDate.Value;

                if (dto.DurationMinutes.HasValue)
                    lesson.DurationMinutes = dto.DurationMinutes.Value;

                if (dto.Notes != null)
                    lesson.Notes = dto.Notes;

                if (dto.IsAbsent.HasValue)
                    lesson.IsAbsent = dto.IsAbsent.Value;

                if (dto.Evaluation.HasValue)
                    lesson.Evaluation = dto.Evaluation.Value;

                lesson.UpdatedAt = DateTime.UtcNow;
                lesson.UpdatedBy = userId;

                _unitOfWork.Repository<Lesson>().Update(lesson);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في تحديث الدرس: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
                if (lesson == null)
                    return Result<bool>.Failure("الدرس غير موجود");

                lesson.IsDeleted = true;
                lesson.DeletedAt = DateTime.UtcNow;
                lesson.DeletedBy = id.ToString();

                _unitOfWork.Repository<Lesson>().Update(lesson);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"خطأ في حذف الدرس: {ex.Message}");
            }
        }

        #region Private Helpers

        private IQueryable<Lesson> ApplyRoleFilter(IQueryable<Lesson> query, string role, string userId)
        {
            if (role == "Teacher")
                return query.Where(l => l.Teacher.UserId == userId);

            if (role == "Admin")
                return query.Where(l => l.Teacher.Supervisor != null && l.Teacher.Supervisor.UserId == userId);

            return query; // SuperAdmin sees all
        }

        private async Task<Teacher?> GetTeacherByUserId(string userId)
        {
            var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
            return teachers.FirstOrDefault();
        }

        private async Task<Supervisor?> GetSupervisorByUserId(string userId)
        {
            var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
            return supervisors.FirstOrDefault();
        }

        private static PaginatedResponseDto<LessonResponseDto> EmptyPaginatedResponse(int pageNumber, int pageSize)
        {
            return new PaginatedResponseDto<LessonResponseDto>
            {
                Items = new List<LessonResponseDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                PagesCount = 0
            };
        }

        #endregion
    }
}
