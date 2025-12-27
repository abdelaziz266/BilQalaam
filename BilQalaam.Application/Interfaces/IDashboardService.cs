using BilQalaam.Application.DTOs.Dashboard;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard statistics based on user role
        /// SuperAdmin: all counts
        /// Admin: teachers, families, students, lessons (of their supervisor)
        /// Teacher: students, lessons (of their teacher)
        /// </summary>
        Task<Result<DashboardDto>> GetDashboardAsync(string role, string userId);
    }
}
