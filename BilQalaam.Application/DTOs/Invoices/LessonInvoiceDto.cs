using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Invoices
{
    public class LessonInvoiceDto
    {
        public int LessonId { get; set; }

        // ??  «—ÌŒ «·œ—”
        public DateTime LessonDate { get; set; }

        // ??û?? »Ì«‰«  «·ÿ«·»
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // ??û?? «”„ «·„⁄·„ (Ìı—Ã⁄ ›ﬁÿ ≈–« ﬂ«‰ «·„” Œœ„ Admin √Ê Supervisor)
        public string? TeacherName { get; set; }

        // ?? „œ… «·œ—” »«·”«⁄« 
        public decimal DurationHours { get; set; }

        // ?  ﬁÌÌ„ «·ÿ«·» (enum „À· LessonResponseDto)
        public LessonEvaluation? Evaluation { get; set; }
    }
}
