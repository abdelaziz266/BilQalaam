using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Supervisors
{
    public class UpdateSupervisorDto
    {
        // ?? ÈíÇäÇÊ ÇáíæÒÑ
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // ?? ÈíÇäÇÊ ÇáãÔÑİ
        public string SupervisorName { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }
    }
}
