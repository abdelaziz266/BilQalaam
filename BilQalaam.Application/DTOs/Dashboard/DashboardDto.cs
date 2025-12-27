namespace BilQalaam.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        // SuperAdmin only
        public int? SupervisorsCount { get; set; }
        
        // SuperAdmin + Admin
        public int? TeachersCount { get; set; }
        public int? FamiliesCount { get; set; }
        
        // SuperAdmin + Admin + Teacher
        public int? StudentsCount { get; set; }
        public int? TotalLessonsCount { get; set; }
        public int? CurrentMonthLessonsCount { get; set; }
    }
}
