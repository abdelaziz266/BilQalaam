using AutoMapper;
using BilQalaam.Application.DTOs.Invoices;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ?? ›« Ê—… «·⁄«∆·…
        public async Task<FamilyInvoiceDto?> GetFamilyInvoiceAsync(int familyId, DateTime fromDate, DateTime toDate)
        {
            var family = await _unitOfWork.Repository<Family>().GetByIdAsync(familyId);
            if (family == null) return null;

            var lessons = await _unitOfWork.Repository<Lesson>()
                .FindAsync(l => l.FamilyId == familyId 
                    && l.LessonDate >= fromDate 
                    && l.LessonDate <= toDate);

            var lessonsList = lessons.ToList();

            // Ã·» «·ÿ·«»
            var studentIds = lessonsList.Select(l => l.StudentId).Distinct().ToList();
            var students = await _unitOfWork.Repository<Student>()
                .FindAsync(s => studentIds.Contains(s.Id));

            // Ã·» «·„⁄·„Ì‰
            var teacherIds = lessonsList.Select(l => l.TeacherId).Distinct().ToList();
            var teachers = await _unitOfWork.Repository<Teacher>()
                .FindAsync(t => teacherIds.Contains(t.Id));

            var studentsDict = students.ToDictionary(s => s.Id);
            var teachersDict = teachers.ToDictionary(t => t.Id);

            var invoice = new FamilyInvoiceDto
            {
                FamilyId = familyId,
                FamilyName = family.FamilyName,
                FromDate = fromDate,
                ToDate = toDate,
                Currency = family.Currency,
                Students = new List<StudentInvoiceDetailDto>()
            };

            //  Ã„Ì⁄ «·œ—Ê” Õ”» «·ÿ«·»
            var groupedByStudent = lessonsList.GroupBy(l => l.StudentId);

            foreach (var studentGroup in groupedByStudent)
            {
                var studentId = studentGroup.Key;
                var studentLessons = studentGroup.ToList();
                var student = studentsDict.GetValueOrDefault(studentId);
                var teacher = student != null ? teachersDict.GetValueOrDefault(student.TeacherId) : null;

                var studentDetail = new StudentInvoiceDetailDto
                {
                    StudentId = studentId,
                    StudentName = student?.StudentName ?? "€Ì— „⁄—Ê›",
                    TeacherName = teacher?.TeacherName ?? "€Ì— „⁄—Ê›",
                    TotalLessons = studentLessons.Count,
                    TotalHours = studentLessons.Sum(l => l.DurationMinutes) / 60m,
                    HourlyRate = studentLessons.FirstOrDefault()?.StudentHourlyRate ?? 0,
                    Lessons = studentLessons.Select(l => new LessonDetailDto
                    {
                        LessonId = l.Id,
                        LessonDate = l.LessonDate,
                        DurationMinutes = l.DurationMinutes,
                        Hours = l.DurationMinutes / 60m,
                        Amount = (l.DurationMinutes / 60m) * l.StudentHourlyRate,
                        Notes = l.Notes,
                        Evaluation = l.Evaluation
                    }).OrderBy(l => l.LessonDate).ToList()
                };

                studentDetail.TotalAmount = studentDetail.TotalHours * studentDetail.HourlyRate;
                invoice.Students.Add(studentDetail);
            }

            invoice.TotalLessons = invoice.Students.Sum(s => s.TotalLessons);
            invoice.TotalHours = invoice.Students.Sum(s => s.TotalHours);
            invoice.TotalAmount = invoice.Students.Sum(s => s.TotalAmount);

            return invoice;
        }

        // ?? ›« Ê—… «·„⁄·„
        public async Task<TeacherInvoiceDto?> GetTeacherInvoiceAsync(int teacherId, DateTime fromDate, DateTime toDate)
        {
            var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
            if (teacher == null) return null;

            var lessons = await _unitOfWork.Repository<Lesson>()
                .FindAsync(l => l.TeacherId == teacherId 
                    && l.LessonDate >= fromDate 
                    && l.LessonDate <= toDate);

            var lessonsList = lessons.ToList();

            // Ã·» «·ÿ·«»
            var studentIds = lessonsList.Select(l => l.StudentId).Distinct().ToList();
            var students = await _unitOfWork.Repository<Student>()
                .FindAsync(s => studentIds.Contains(s.Id));

            // Ã·» «·⁄«∆·« 
            var familyIds = lessonsList.Select(l => l.FamilyId).Distinct().ToList();
            var families = await _unitOfWork.Repository<Family>()
                .FindAsync(f => familyIds.Contains(f.Id));

            var studentsDict = students.ToDictionary(s => s.Id);
            var familiesDict = families.ToDictionary(f => f.Id);

            var invoice = new TeacherInvoiceDto
            {
                TeacherId = teacherId,
                TeacherName = teacher.TeacherName,
                FromDate = fromDate,
                ToDate = toDate,
                Currency = teacher.Currency,
                Students = new List<TeacherStudentDetailDto>()
            };

            //  Ã„Ì⁄ «·œ—Ê” Õ”» «·ÿ«·»
            var groupedByStudent = lessonsList.GroupBy(l => l.StudentId);

            foreach (var studentGroup in groupedByStudent)
            {
                var studentId = studentGroup.Key;
                var studentLessons = studentGroup.ToList();
                var student = studentsDict.GetValueOrDefault(studentId);
                var family = student != null ? familiesDict.GetValueOrDefault(student.FamilyId ?? 0) : null;

                var studentDetail = new TeacherStudentDetailDto
                {
                    StudentId = studentId,
                    StudentName = student?.StudentName ?? "€Ì— „⁄—Ê›",
                    FamilyName = family?.FamilyName ?? "€Ì— „⁄—Ê›",
                    TotalLessons = studentLessons.Count,
                    TotalHours = studentLessons.Sum(l => l.DurationMinutes) / 60m,
                    HourlyRate = studentLessons.FirstOrDefault()?.TeacherHourlyRate ?? 0,
                    Lessons = studentLessons.Select(l => new LessonDetailDto
                    {
                        LessonId = l.Id,
                        LessonDate = l.LessonDate,
                        DurationMinutes = l.DurationMinutes,
                        Hours = l.DurationMinutes / 60m,
                        Amount = (l.DurationMinutes / 60m) * l.TeacherHourlyRate,
                        Notes = l.Notes,
                        Evaluation = l.Evaluation
                    }).OrderBy(l => l.LessonDate).ToList()
                };

                studentDetail.TotalEarnings = studentDetail.TotalHours * studentDetail.HourlyRate;
                invoice.Students.Add(studentDetail);
            }

            invoice.TotalLessons = invoice.Students.Sum(s => s.TotalLessons);
            invoice.TotalHours = invoice.Students.Sum(s => s.TotalHours);
            invoice.TotalEarnings = invoice.Students.Sum(s => s.TotalEarnings);

            return invoice;
        }

        // ?? ›« Ê—… «·„‘—›
        public async Task<SupervisorInvoiceDto?> GetSupervisorInvoiceAsync(int supervisorId, DateTime fromDate, DateTime toDate)
        {
            var supervisor = await _unitOfWork.Repository<Supervisor>().GetByIdAsync(supervisorId);
            if (supervisor == null) return null;

            // Ã·» «·⁄«∆·«  «· «»⁄… ··„‘—›
            var families = await _unitOfWork.Repository<Family>()
                .FindAsync(f => f.SupervisorId == supervisorId);
            var familyIds = families.Select(f => f.Id).ToList();

            // Ã·» «·„⁄·„Ì‰ «· «»⁄Ì‰ ··„‘—›
            var teachers = await _unitOfWork.Repository<Teacher>()
                .FindAsync(t => t.SupervisorId == supervisorId);
            var teacherIds = teachers.Select(t => t.Id).ToList();

            // Ã·» «·œ—Ê”
            var lessons = await _unitOfWork.Repository<Lesson>()
                .FindAsync(l => (familyIds.Contains(l.FamilyId) || teacherIds.Contains(l.TeacherId))
                    && l.LessonDate >= fromDate 
                    && l.LessonDate <= toDate);

            var lessonsList = lessons.ToList();

            // Ã·» «·ÿ·«»
            var studentIds = lessonsList.Select(l => l.StudentId).Distinct().ToList();
            var students = await _unitOfWork.Repository<Student>()
                .FindAsync(s => studentIds.Contains(s.Id));

            var familiesDict = families.ToDictionary(f => f.Id);
            var teachersDict = teachers.ToDictionary(t => t.Id);

            var invoice = new SupervisorInvoiceDto
            {
                SupervisorId = supervisorId,
                SupervisorName = supervisor.SupervisorName,
                FromDate = fromDate,
                ToDate = toDate,
                Currency = supervisor.Currency,
                TotalFamilies = familyIds.Distinct().Count(),
                TotalTeachers = teacherIds.Distinct().Count(),
                TotalStudents = studentIds.Distinct().Count(),
                Families = new List<SupervisorFamilyDetailDto>(),
                Teachers = new List<SupervisorTeacherDetailDto>()
            };

            //  Ã„Ì⁄ Õ”» «·⁄«∆·« 
            var groupedByFamily = lessonsList.GroupBy(l => l.FamilyId);
            foreach (var familyGroup in groupedByFamily)
            {
                var familyId = familyGroup.Key;
                var familyLessons = familyGroup.ToList();
                var family = familiesDict.GetValueOrDefault(familyId);

                invoice.Families.Add(new SupervisorFamilyDetailDto
                {
                    FamilyId = familyId,
                    FamilyName = family?.FamilyName ?? "€Ì— „⁄—Ê›",
                    TotalStudents = familyLessons.Select(l => l.StudentId).Distinct().Count(),
                    TotalLessons = familyLessons.Count,
                    TotalHours = familyLessons.Sum(l => l.DurationMinutes) / 60m,
                    TotalAmount = familyLessons.Sum(l => (l.DurationMinutes / 60m) * l.StudentHourlyRate)
                });
            }

            //  Ã„Ì⁄ Õ”» «·„⁄·„Ì‰
            var groupedByTeacher = lessonsList.GroupBy(l => l.TeacherId);
            foreach (var teacherGroup in groupedByTeacher)
            {
                var teacherId = teacherGroup.Key;
                var teacherLessons = teacherGroup.ToList();
                var teacher = teachersDict.GetValueOrDefault(teacherId);

                invoice.Teachers.Add(new SupervisorTeacherDetailDto
                {
                    TeacherId = teacherId,
                    TeacherName = teacher?.TeacherName ?? "€Ì— „⁄—Ê›",
                    TotalStudents = teacherLessons.Select(l => l.StudentId).Distinct().Count(),
                    TotalLessons = teacherLessons.Count,
                    TotalHours = teacherLessons.Sum(l => l.DurationMinutes) / 60m,
                    TotalEarnings = teacherLessons.Sum(l => (l.DurationMinutes / 60m) * l.TeacherHourlyRate)
                });
            }

            invoice.TotalLessons = lessonsList.Count;
            invoice.TotalHours = lessonsList.Sum(l => l.DurationMinutes) / 60m;
            invoice.TotalFamilyAmount = invoice.Families.Sum(f => f.TotalAmount);
            invoice.TotalTeacherEarnings = invoice.Teachers.Sum(t => t.TotalEarnings);

            return invoice;
        }

        // ?? Ã·» ﬂ· ›Ê« Ì— «·⁄«∆·« 
        public async Task<IEnumerable<FamilyInvoiceDto>> GetAllFamilyInvoicesAsync(DateTime fromDate, DateTime toDate, int? supervisorId = null)
        {
            var familiesQuery = await _unitOfWork.Repository<Family>().GetAllAsync();
            var families = familiesQuery.ToList();

            if (supervisorId.HasValue)
                families = families.Where(f => f.SupervisorId == supervisorId.Value).ToList();

            var invoices = new List<FamilyInvoiceDto>();

            foreach (var family in families)
            {
                var invoice = await GetFamilyInvoiceAsync(family.Id, fromDate, toDate);
                if (invoice != null && invoice.TotalLessons > 0)
                    invoices.Add(invoice);
            }

            return invoices;
        }

        // ?? Ã·» ﬂ· ›Ê« Ì— «·„⁄·„Ì‰
        public async Task<IEnumerable<TeacherInvoiceDto>> GetAllTeacherInvoicesAsync(DateTime fromDate, DateTime toDate, int? supervisorId = null)
        {
            var teachersQuery = await _unitOfWork.Repository<Teacher>().GetAllAsync();
            var teachers = teachersQuery.ToList();

            if (supervisorId.HasValue)
                teachers = teachers.Where(t => t.SupervisorId == supervisorId.Value).ToList();

            var invoices = new List<TeacherInvoiceDto>();

            foreach (var teacher in teachers)
            {
                var invoice = await GetTeacherInvoiceAsync(teacher.Id, fromDate, toDate);
                if (invoice != null && invoice.TotalLessons > 0)
                    invoices.Add(invoice);
            }

            return invoices;
        }

        /// <summary>
        /// Get all lessons (invoices) between date range
        /// Default: from first day of current month to today
        /// userRole: to determine if we should include teacher name
        /// </summary>
        public async Task<Result<List<LessonInvoiceDto>>> GetAllLessonsInvoicesAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userRole = null)
        {
            try
            {
                //  ÕœÌœ «· Ê«—ÌŒ «·«› —«÷Ì…
                var today = DateTime.Now.Date;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                fromDate ??= firstDayOfMonth;
                toDate ??= today;

                //  √ﬂœ „‰ √‰ fromDate Ì”»ﬁ toDate
                if (fromDate > toDate)
                {
                    return Result<List<LessonInvoiceDto>>.Failure(" «—ÌŒ «·»œ«Ì… ÌÃ» √‰ ÌﬂÊ‰ ﬁ»·  «—ÌŒ «·‰Â«Ì…");
                }

                // ÃÌ» ﬂ· «·œ—Ê” ›Ì ‰ÿ«ﬁ «· «—ÌŒ
                var lessons = await _unitOfWork.Repository<Lesson>()
                    .Query()
                    .Include(l => l.Teacher)
                    .Include(l => l.Student)
                    .Where(l => l.LessonDate >= fromDate && l.LessonDate <= toDate && !l.IsDeleted)
                    .OrderByDescending(l => l.LessonDate)
                    .ToListAsync();

                if (!lessons.Any())
                {
                    return Result<List<LessonInvoiceDto>>.Success(new List<LessonInvoiceDto>()); 
                }

                //  ÕÊÌ· «·œ—Ê” ≈·Ï LessonInvoiceDto
                // «ŸÂ— «”„ «·„⁄·„ ›ﬁÿ ·‹ Admin √Ê Supervisor
                var isAdminOrSupervisor = userRole == "SuperAdmin" || userRole == "Admin";

                var lessonInvoices = lessons.Select(l => new LessonInvoiceDto
                {
                    LessonId = l.Id,
                    LessonDate = l.LessonDate,
                    StudentId = l.StudentId,
                    StudentName = l.Student?.StudentName ?? "«”„ €Ì— „ «Õ",
                    TeacherName = isAdminOrSupervisor ? l.Teacher?.TeacherName : null,
                    DurationHours = l.DurationMinutes ,
                    Evaluation = l.Evaluation
                }).ToList();

                return Result<List<LessonInvoiceDto>>.Success(lessonInvoices);
            }
            catch (Exception ex)
            {
                return Result<List<LessonInvoiceDto>>.Failure($"Œÿ√ ›Ì Ã·» «·œ—Ê”: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all lessons (invoices) with role-based filtering and teacher summary
        /// </summary>
        public async Task<Result<LessonsInvoicesResponseDto>> GetLessonsInvoicesWithSummaryAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? teacherIdFilter = null,
            int? familyIdFilter = null,
            string? userRole = null,
            string? userId = null)
        {
            try
            {
                //  ÕœÌœ «· Ê«—ÌŒ «·«› —«÷Ì…
                var today = DateTime.Now.Date;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                fromDate ??= firstDayOfMonth;
                toDate ??= today;

                //  √ﬂœ „‰ √‰ fromDate Ì”»ﬁ toDate
                if (fromDate > toDate)
                {
                    return Result<LessonsInvoicesResponseDto>.Failure(" «—ÌŒ «·»œ«Ì… ÌÃ» √‰ ÌﬂÊ‰ ﬁ»·  «—ÌŒ «·‰Â«Ì…");
                }

                // ÃÌ» ﬂ· «·œ—Ê”
                var lessonsQuery = _unitOfWork.Repository<Lesson>()
                    .Query()
                    .Include(l => l.Teacher)
                    .Include(l => l.Student)
                    .Where(l => l.LessonDate >= fromDate && l.LessonDate <= toDate && !l.IsDeleted);

                //  ÿ»Ìﬁ «·›·« — Õ”» «·‹ Role
                if (userRole == "Admin")
                {
                    // Admin: Ì‘Ê› ›ﬁÿ œ—Ê” «·„⁄·„Ì‰ «· «»⁄Ì‰ ·Â
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„‘—›");
                    lessonsQuery = lessonsQuery.Where(l => l.Teacher.SupervisorId == supervisor.Id);
                }
                else if (userRole == "Teacher")
                {
                    // Teacher: Ì‘Ê› ›ﬁÿ œ—Ê”Â
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„⁄·„");
                    lessonsQuery = lessonsQuery.Where(l => l.TeacherId == teacher.Id);
                }
                else if (userRole == "Family")
                {
                    // Family: Ì‘Ê› ›ﬁÿ œ—Ê” ÿ·«»Â„
                    var family = await GetFamilyByUserId(userId);
                    if (family == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·⁄«∆·…");
                    lessonsQuery = lessonsQuery.Where(l => l.FamilyId == family.Id);
                }
                // SuperAdmin: Ì‘Ê› ﬂ· Õ«Ã…

                //  ÿ»Ìﬁ «·›·« — «·≈÷«›Ì…
                if (teacherIdFilter.HasValue && (userRole == "SuperAdmin" || userRole == "Admin"))
                {
                    lessonsQuery = lessonsQuery.Where(l => l.TeacherId == teacherIdFilter.Value);
                }

                if (familyIdFilter.HasValue && (userRole == "SuperAdmin" || userRole == "Admin" || userRole == "Teacher"))
                {
                    lessonsQuery = lessonsQuery.Where(l => l.FamilyId == familyIdFilter.Value);
                }

                var lessons = await lessonsQuery.OrderByDescending(l => l.LessonDate).ToListAsync();

                if (!lessons.Any())
                {
                    return Result<LessonsInvoicesResponseDto>.Success(new LessonsInvoicesResponseDto
                    {
                        Lessons = new List<LessonInvoiceDto>(),
                        TotalLessons = 0,
                        TotalHours = 0,
                        TotalAmount = 0
                    });
                }

                //  ÕÊÌ· «·œ—Ê” ≈·Ï LessonInvoiceDto
                var isAdminOrSupervisor = userRole == "SuperAdmin" || userRole == "Admin";

                var lessonInvoices = lessons.Select(l => new LessonInvoiceDto
                {
                    LessonId = l.Id,
                    LessonDate = l.LessonDate,
                    StudentId = l.StudentId,
                    StudentName = l.Student?.StudentName ?? "«”„ €Ì— „ «Õ",
                    TeacherName = isAdminOrSupervisor ? l.Teacher?.TeacherName : null,
                    DurationHours = l.DurationMinutes ,
                    Evaluation = l.Evaluation
                }).ToList();

                // ÃÌ» »Ì«‰«  «·„⁄·„ ≈–«  „ «·›· —… »Â √Ê ≈–« ﬂ«‰ «·„” Œœ„ „⁄·„
                TeacherSummaryDto? teacherSummary = null;

                if (teacherIdFilter.HasValue)
                {
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherIdFilter.Value);
                    if (teacher != null)
                    {
                        var teacherLessons = lessons.Where(l => l.TeacherId == teacher.Id).ToList();
                        teacherSummary = new TeacherSummaryDto
                        {
                            TeacherId = teacher.Id,
                            TeacherName = teacher.TeacherName,
                            HourlyRate = teacher.HourlyRate,
                            Currency = teacher.Currency.ToString(),
                            TotalHours = teacherLessons.Sum(l => l.DurationMinutes),
                            TotalEarnings = teacherLessons.Sum(l => (l.DurationMinutes / 60m) * l.TeacherHourlyRate)
                        };
                    }
                }
                else if (userRole == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher != null)
                    {
                        teacherSummary = new TeacherSummaryDto
                        {
                            TeacherId = teacher.Id,
                            TeacherName = teacher.TeacherName,
                            HourlyRate = teacher.HourlyRate,
                            Currency = teacher.Currency.ToString(),
                            TotalHours = lessons.Sum(l => l.DurationMinutes),
                            TotalEarnings = lessons.Sum(l => (l.DurationMinutes / 60m) * l.TeacherHourlyRate)
                        };
                    }
                }

                var response = new LessonsInvoicesResponseDto
                {
                    Lessons = lessonInvoices,
                    TeacherSummary = teacherSummary,
                    TotalLessons = lessonInvoices.Count,
                    TotalHours = lessonInvoices.Sum(l => l.DurationHours),
                    TotalAmount = lessonInvoices.Sum(l => (l.DurationHours * (lessons.FirstOrDefault(x => x.Id == l.LessonId)?.StudentHourlyRate ?? 0)))
                };

                return Result<LessonsInvoicesResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<LessonsInvoicesResponseDto>.Failure($"Œÿ√ ›Ì Ã·» «·œ—Ê”: {ex.Message}");
            }
        }

        #region Private Helpers

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

        private async Task<Family?> GetFamilyByUserId(string userId)
        {
            var families = await _unitOfWork.Repository<Family>().FindAsync(f => f.UserId == userId);
            return families.FirstOrDefault();
        }

        #endregion
    }
}
