using BilQalaam.Domain.Common;
using BilQalaam.Domain.Enums;

namespace BilQalaam.Domain.Entities
{
    public class Family : Base
    {
        // ?? ÈíÇäÇÊ ÇáÚÇÆáÉ ÇáÃÓÇÓíÉ
        public string FamilyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÇáãÔÑİ ÇáÊÇÈÚ áå (ãÑÊÈØ ÈÌÏæá Supervisors)
        public int? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }

        // ?? ÈíÇäÇÊ ÇáÓÚÑ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÇáÇÑÊÈÇØ ãÚ User (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
