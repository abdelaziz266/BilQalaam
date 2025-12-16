using BilQalaam.Domain.Common;
using BilQalaam.Domain.Entities;

public class Lesson:Base
{
    public string? FamilyId { get; set; } = null!;
    public ApplicationUser Family { get; set; } = null!;

    public string StudentId { get; set; } = null!;
    public ApplicationUser Student { get; set; } = null!;

    public DateTime LessonDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
