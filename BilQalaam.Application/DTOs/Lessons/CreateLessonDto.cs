using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class CreateLessonDto
    {
        public string FamilyId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public LessonEvaluation? Evaluation { get; set; }
    }
}
