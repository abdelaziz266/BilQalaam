using AutoMapper;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;

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

        public async Task<(IEnumerable<LessonResponseDto>, int)> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .GetAllAsync();

                var totalCount = lessons.Count();
                var paginatedLessons = lessons
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (_mapper.Map<IEnumerable<LessonResponseDto>>(paginatedLessons), totalCount);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب الدروس: {ex.Message}" });
            }
        }

        public async Task<LessonResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                var lesson = await _unitOfWork
                    .Repository<Lesson>()
                    .GetByIdAsync(id);

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
                    .FindAsync(l => l.TeacherId == teacherId);

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

        public async Task<IEnumerable<LessonResponseDto>> GetByFamilyIdAsync(int familyId)
        {
            try
            {
                var lessons = await _unitOfWork
                    .Repository<Lesson>()
                    .FindAsync(l => l.FamilyId == familyId);

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
                    .FindAsync(l => l.StudentId == studentId);

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
                    .FindAsync(l => l.LessonDate >= fromDate && l.LessonDate <= toDate);

                return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
            }
            catch (Exception ex)
            {
                throw new ValidationException(new List<string> { $"خطأ في جلب الدروس حسب التاريخ: {ex.Message}" });
            }
        }

        public async Task<int> CreateAsync(CreateLessonDto dto, int teacherId, string createdByUserId)
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
                    FamilyId = student.FamilyId,
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

        public async Task<bool> UpdateAsync(int id, UpdateLessonDto dto, string updatedByUserId)
        {
            try
            {
                var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
                if (lesson == null) return false;

                if (dto.StudentId.HasValue && dto.StudentId.Value != lesson.StudentId)
                {
                    var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId.Value);
                    if (student == null) return false;

                    lesson.StudentId = dto.StudentId.Value;
                    lesson.FamilyId = student.FamilyId;
                    lesson.StudentHourlyRate = student.HourlyRate;
                }

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
    }
}
