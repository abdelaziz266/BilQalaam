using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class UpdateLessonDto
    {
        public DateTime? LessonDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public LessonEvaluation? Evaluation { get; set; }
    }
}
