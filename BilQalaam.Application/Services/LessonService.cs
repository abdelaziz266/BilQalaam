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

        public async Task<IEnumerable<LessonResponseDto>> GetAllAsync()
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<LessonResponseDto?> GetByIdAsync(int id)
        {
            var lesson = await _unitOfWork
                .Repository<Lesson>()
                .GetByIdAsync(id);

            return lesson == null ? null : _mapper.Map<LessonResponseDto>(lesson);
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByTeacherIdAsync(int teacherId)
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .FindAsync(l => l.TeacherId == teacherId);

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByFamilyIdAsync(int familyId)
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .FindAsync(l => l.FamilyId == familyId);

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByStudentIdAsync(int studentId)
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .FindAsync(l => l.StudentId == studentId);

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<IEnumerable<LessonResponseDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .FindAsync(l => l.LessonDate >= fromDate && l.LessonDate <= toDate);

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<int> CreateAsync(CreateLessonDto dto, int teacherId, string createdByUserId)
        {
            // جلب بيانات الطالب
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);
            if (student == null)
                throw new ValidationException(new List<string> { "الطالب غير موجود" });

            // التحقق أن المعلم هو معلم هذا الطالب
            if (student.TeacherId != teacherId)
                throw new ValidationException(new List<string> { "هذا الطالب ليس من طلابك" });

            // جلب بيانات المعلم
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

        public async Task<bool> UpdateAsync(int id, UpdateLessonDto dto, string updatedByUserId)
        {
            var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
            if (lesson == null) return false;

            // لو عاوز يغير الطالب
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

        public async Task<bool> DeleteAsync(int id)
        {
            var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
            if (lesson == null) return false;

            _unitOfWork.Repository<Lesson>().Delete(lesson);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
