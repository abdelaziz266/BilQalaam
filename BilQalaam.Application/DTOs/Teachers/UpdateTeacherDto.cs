using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Teachers
{
    public class UpdateTeacherDto
    {
        // ?? ÈíÇäÇÊ ÇáíæÒÑ
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // ?? ÈíÇäÇÊ ÇáãÚáã
        public string TeacherName { get; set; } = string.Empty;

        // ?? ÇáãÔÑİ ÇáÊÇÈÚ áå (ID ãä ÌÏæá Supervisors)
        public int? SupervisorId { get; set; }

        // ?? ÈíÇäÇÊ ÇáÓÚÑ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }
    }
}
