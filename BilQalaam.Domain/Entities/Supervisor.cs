using BilQalaam.Domain.Common;
using BilQalaam.Domain.Enums;

namespace BilQalaam.Domain.Entities
{
    public class Supervisor : Base
    {
        // ?? ÈíÇäÇÊ ÇáãÔÑİ ÇáÃÓÇÓíÉ
        public string SupervisorName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÇáÇÑÊÈÇØ ãÚ User (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
