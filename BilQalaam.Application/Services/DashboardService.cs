using BilQalaam.Application.DTOs.Dashboard;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DashboardDto>> GetDashboardAsync(string role, string userId)
        {
            try
            {
                var dashboard = new DashboardDto();

                // SuperAdmin: Ì‘Ê› «·ﬂ·
                if (role == "SuperAdmin")
                {
                    dashboard.SupervisorsCount = await _unitOfWork.Repository<Supervisor>().Query().Where(x=>x.IsDeleted!=true).CountAsync();
                    dashboard.TeachersCount = await _unitOfWork.Repository<Teacher>().Query().Where(x => x.IsDeleted != true).CountAsync();
                    dashboard.FamiliesCount = await _unitOfWork.Repository<Family>().Query().Where(x => x.IsDeleted != true).CountAsync();
                    dashboard.StudentsCount = await _unitOfWork.Repository<Student>().Query().Where(x => x.IsDeleted != true).CountAsync();
                    dashboard.TotalLessonsCount = await _unitOfWork.Repository<Lesson>().Query().Where(x => x.IsDeleted != true).CountAsync();
                }
                // Admin: Ì‘Ê› «·„⁄·„Ì‰ Ê«·⁄«∆·«  Ê«·ÿ·«» Ê«·œ—Ê” «·„— »ÿÌ‰ »Â
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<DashboardDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„‘—›");

                    dashboard.TeachersCount = await _unitOfWork.Repository<Teacher>().Query()
                        .Where(t => t.SupervisorId == supervisor.Id)
                        .CountAsync();

                    dashboard.FamiliesCount = await _unitOfWork.Repository<Family>().Query()
                        .Where(f => f.SupervisorId == supervisor.Id)
                        .CountAsync();

                    dashboard.StudentsCount = await _unitOfWork.Repository<Student>().Query()
                        .Include(s => s.Teacher)
                        .Where(s => s.Teacher.SupervisorId == supervisor.Id)
                        .CountAsync();

                    dashboard.TotalLessonsCount = await _unitOfWork.Repository<Lesson>().Query()
                        .Where(l => l.SupervisorId == supervisor.Id)
                        .CountAsync();
                }
                // Teacher: Ì‘Ê› ÿ·«»Â Ê«·œ—Ê” «·Œ«’… »Â ›ﬁÿ
                else if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<DashboardDto>.Failure("·„ Ì „ «·⁄ÀÊ— ⁄·Ï »Ì«‰«  «·„⁄·„");

                    dashboard.StudentsCount = await _unitOfWork.Repository<Student>().Query()
                        .Where(s => s.TeacherId == teacher.Id)
                        .CountAsync();

                    dashboard.TotalLessonsCount = await _unitOfWork.Repository<Lesson>().Query()
                        .Where(l => l.TeacherId == teacher.Id)
                        .CountAsync();
                }

                // «·œ—Ê” Œ·«· «·‘Â— «·Õ«·Ì: «·ﬂ· Ì‘Ê›Â«
                var currentDate = DateTime.UtcNow;
                var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                if (role == "SuperAdmin")
                {
                    dashboard.CurrentMonthLessonsCount = await _unitOfWork.Repository<Lesson>().Query()
                        .Where(l => l.LessonDate >= monthStart && l.LessonDate <= monthEnd)
                        .CountAsync();
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor != null)
                    {
                        dashboard.CurrentMonthLessonsCount = await _unitOfWork.Repository<Lesson>().Query()
                            .Where(l => l.SupervisorId == supervisor.Id && 
                                   l.LessonDate >= monthStart && l.LessonDate <= monthEnd)
                            .CountAsync();
                    }
                }
                else if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher != null)
                    {
                        dashboard.CurrentMonthLessonsCount = await _unitOfWork.Repository<Lesson>().Query()
                            .Where(l => l.TeacherId == teacher.Id && 
                                   l.LessonDate >= monthStart && l.LessonDate <= monthEnd)
                            .CountAsync();
                    }
                }

                return Result<DashboardDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                return Result<DashboardDto>.Failure($"Œÿ√ ›Ì Ã·» »Ì«‰«  ·ÊÕ… «·»Ì«‰« : {ex.Message}");
            }
        }

        #region Private Helpers

        private async Task<Supervisor?> GetSupervisorByUserId(string userId)
        {
            var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
            return supervisors.FirstOrDefault();
        }

        private async Task<Teacher?> GetTeacherByUserId(string userId)
        {
            var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
            return teachers.FirstOrDefault();
        }

        #endregion
    }
}
