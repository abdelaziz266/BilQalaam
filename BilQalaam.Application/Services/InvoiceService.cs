using AutoMapper;
using BilQalaam.Application.DTOs.Invoices;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrencyService _currencyService;

        public InvoiceService(IUnitOfWork unitOfWork, ICurrencyService currencyService, IMapper mapper = null)
        {
            _unitOfWork = unitOfWork;
            _currencyService = currencyService;
            _mapper = mapper;
        }

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
                var today = DateTime.Now.Date;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                fromDate ??= firstDayOfMonth;
                toDate ??= today;
                if (fromDate > toDate)
                {
                    return Result<LessonsInvoicesResponseDto>.Failure(" «—ÌŒ «·»œ«Ì… ÌÃ» √‰ ÌﬂÊ‰ ﬁ»·  «—ÌŒ «·‰Â«Ì…");
                }
                var lessonsQuery = _unitOfWork.Repository<Lesson>()
                    .Query()
                    .Include(l => l.Teacher)
                    .Include(l => l.Student)
                    .Where(l =>
                                l.LessonDate.Date >= fromDate.Value.Date &&
                                l.LessonDate.Date <= toDate.Value.Date &&
                                l.IsDeleted != true);
                if (userRole == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„‘—›");
                    lessonsQuery = lessonsQuery.Where(l => l.Teacher.SupervisorId == supervisor.Id);
                }
                else if (userRole == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„⁄·„");
                    lessonsQuery = lessonsQuery.Where(l => l.TeacherId == teacher.Id);
                }
                else if (userRole == "Family")
                {
                    var family = await GetFamilyByUserId(userId);
                    if (family == null)
                        return Result<LessonsInvoicesResponseDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·⁄«∆·…");
                    lessonsQuery = lessonsQuery.Where(l => l.FamilyId == family.Id);
                }
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
                var isAdminOrSupervisor = userRole == "SuperAdmin" || userRole == "Admin";

                var lessonInvoices = lessons.Select(l => new LessonInvoiceDto
                {
                    LessonId = l.Id,
                    LessonDate = l.LessonDate,
                    StudentId = l.StudentId,
                    StudentName = l.Student?.StudentName ?? "«”„ €Ì— „ «Õ",
                    TeacherName = isAdminOrSupervisor ? l.Teacher?.TeacherName : null,
                    DurationHours = l.DurationMinutes / 60m,
                    Evaluation = l.Evaluation
                }).ToList();
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
                            TotalHours = teacherLessons.Sum(l => l.DurationMinutes / 60m),
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
                            TotalHours = lessons.Sum(l => l.DurationMinutes / 60m),
                            TotalEarnings = lessons.Sum(l => (l.DurationMinutes / 60m) * l.TeacherHourlyRate)
                        };


                    }
                }

                FamilySummaryDto? familySummary = null;
                if (familyIdFilter.HasValue)
                {
                    var family = await _unitOfWork.Repository<Family>().GetByIdAsync(familyIdFilter.Value);
                    if (family != null)
                    {
                        var familyLessons = lessons.Where(l => l.FamilyId == family.Id).ToList();
                        familySummary = new FamilySummaryDto
                        {
                            FamilyId = family.Id,
                            FamilyName = family.FamilyName,
                            HourlyRate = family.HourlyRate,
                            Currency = family.Currency.ToString(),
                            TotalHours = familyLessons.Sum(l => l.DurationMinutes / 60m),
                            TotalCost = familyLessons.Sum(l => (l.DurationMinutes / 60m) * l.StudentHourlyRate)
                        };
                    }
                }
                else if (userRole == "Family")
                {
                    var family = await GetFamilyByUserId(userId);
                    if (family != null)
                    {
                        familySummary = new FamilySummaryDto
                        {
                            FamilyId = family.Id,
                            FamilyName = family.FamilyName,
                            HourlyRate = family.HourlyRate,
                            Currency = family.Currency.ToString(),
                            TotalHours = lessons.Sum(l => l.DurationMinutes / 60m),
                            TotalCost = lessons.Sum(l => (l.DurationMinutes / 60m) * l.StudentHourlyRate)
                        };
                    }
                }

                var response = new LessonsInvoicesResponseDto
                {
                    Lessons = lessonInvoices,
                    TeacherSummary = teacherSummary,
                    FamilySummary = familySummary,
                    TotalLessons = lessonInvoices.Count,
                    TotalHours = lessonInvoices.Sum(l => l.DurationHours),
                    TotalAmount = lessons.Sum(l => (l.DurationMinutes / 60m) * l.StudentHourlyRate)
                };


                if (userRole == "SuperAdmin")
                {
                    var totalHours = lessons.Sum(l => l.DurationMinutes / 60m);
                    
                    decimal totalAmountEGP = 0;

                    foreach (var l in lessons)
                    {
                        var amount = (l.DurationMinutes / 60m) * l.StudentHourlyRate;
                        totalAmountEGP += await _currencyService.ConvertToEGPAsync(amount, l.Currency);
                    }

                    response.SuperAdminSummary = new SuperAdminSummaryDto
                    {
                        TotalHours = totalHours,
                        TotalAmountUSD = await _currencyService.ConvertToUSDAsync(totalAmountEGP, Currency.EGP),
                        TotalAmountEGP = totalAmountEGP
                    };
                }



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
