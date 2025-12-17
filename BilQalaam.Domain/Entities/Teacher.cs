using BilQalaam.Domain.Common;
using BilQalaam.Domain.Enums;

namespace BilQalaam.Domain.Entities
{
    public class Teacher : Base
    {
        // ?? ÈíÇäÇÊ ÇáãÚáã ÇáÃÓÇÓíÉ
        public string TeacherName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÈíÇäÇÊ ÇáÓÚÑ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÇáãÔÑİ ÇáÊÇÈÚ áå (ãÑÊÈØ ÈÌÏæá Supervisors)
        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }

        // ?? ÇáÇÑÊÈÇØ ãÚ User (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
