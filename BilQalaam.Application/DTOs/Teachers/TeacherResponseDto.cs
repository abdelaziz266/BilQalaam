using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Teachers
{
    public class TeacherResponseDto
    {
        public int Id { get; set; }
        
        // ?? ÈíÇäÇÊ ÇáãÚáã
        public string TeacherName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÊÇÑíÎ ÈÏÁ ÇáÚãá
        public DateTime StartDate { get; set; }

        // ?? ÈíÇäÇÊ ÇáãÔÑİ
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }

        // ?? ÈíÇäÇÊ ÇáãÓÊÎÏã
        public string UserId { get; set; } = string.Empty;

        // ?? ÇáØæÇÈÚ ÇáÒãäíÉ
        public DateTime CreatedAt { get; set; }
    }
}
