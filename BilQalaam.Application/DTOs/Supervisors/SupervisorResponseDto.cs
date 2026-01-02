using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Supervisors
{
    public class SupervisorResponseDto
    {
        public int Id { get; set; }

        // ?? ÈíÇäÇÊ ÇáãÔÑİ
        public string SupervisorName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÊÇÑíÎ ÈÏÁ ÇáÚãá
        public DateTime StartDate { get; set; }

        // ?? ÇáÊæÇÑíÎ
        public DateTime CreatedAt { get; set; }
    }
}
