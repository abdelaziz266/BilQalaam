using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class LessonResponseDto
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public LessonEvaluation? Evaluation { get; set; }
    }
}
