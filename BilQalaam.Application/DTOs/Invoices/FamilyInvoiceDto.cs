using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Invoices
{
    // ?? ›« Ê—… «·⁄«∆·…
    public class FamilyInvoiceDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        // ?? › —… «·›« Ê—…
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // ?? „·Œ’
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }

        // ??  ›«’Ì· «·ÿ·«»
        public List<StudentInvoiceDetailDto> Students { get; set; } = new();
    }

    // ??  ›«’Ì· ›« Ê—… «·ÿ«·»
    public class StudentInvoiceDetailDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<LessonDetailDto> Lessons { get; set; } = new();
    }

    // ??  ›«’Ì· «·œ—”
    public class LessonDetailDto
    {
        public int LessonId { get; set; }
        public DateTime LessonDate { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Hours { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public LessonEvaluation? Evaluation { get; set; }
    }
}
