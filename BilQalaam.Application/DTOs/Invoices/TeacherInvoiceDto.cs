using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Invoices
{
    // ?? ›« Ê—… «·„⁄·„
    public class TeacherInvoiceDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // ?? › —… «·›« Ê—…
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // ?? „·Œ’
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalEarnings { get; set; }
        public Currency Currency { get; set; }

        // ??  ›«’Ì· «·ÿ·«»
        public List<TeacherStudentDetailDto> Students { get; set; } = new();
    }

    // ??  ›«’Ì· ÿ«·» «·„⁄·„
    public class TeacherStudentDetailDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<LessonDetailDto> Lessons { get; set; } = new();
    }
}
