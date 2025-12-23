using BilQalaam.Domain.Common;
using BilQalaam.Domain.Entities;
using BilQalaam.Domain.Enums;

public class Lesson:Base
{
    // 👨‍🎓 الطالب
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    // 👨‍🏫 المعلم (اللي سجل الدرس)
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    // 🧑‍🏫 المشرف
    public int? SupervisorId { get; set; }
    public Supervisor? Supervisor { get; set; }

    // 👨‍👩‍👧 العائلة
    public int FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    // 📅 بيانات الدرس
    public DateTime LessonDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }

    // ⭐ تقييم الدرس
    public LessonEvaluation? Evaluation { get; set; }

    // 💰 المبالغ المحسوبة
    public decimal StudentHourlyRate { get; set; }  // سعر ساعة الطالب وقت الدرس
    public decimal TeacherHourlyRate { get; set; }  // سعر ساعة المعلم وقت الدرس
    public Currency Currency { get; set; }
}
