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

        // ?? ÈíÇäÇÊ ÇáÓÚÑ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?? ÇáãÔÑİ ÇáÊÇÈÚ áå
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }

        // ?? ÈíÇäÇÊ ÇáíæÒÑ
        public string UserId { get; set; } = string.Empty;

        // ?? ÇáÊæÇÑíÎ
        public DateTime CreatedAt { get; set; }
    }
}
