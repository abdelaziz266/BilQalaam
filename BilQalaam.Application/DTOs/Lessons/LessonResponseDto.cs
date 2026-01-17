using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class LessonResponseDto
    {
        public int Id { get; set; }

        // 👨‍🎓 الطالب
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // 👨‍🏫 المعلم
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // 🧑‍🏫 المشرف
        public int? SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;

        // 👨‍👩‍👧 العائلة
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        // 📅 بيانات الدرس
        public DateTime LessonDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }

        // 🛑 الحضور
        public bool IsAbsent { get; set; }

        // ⭐ تقييم الدرس
        public LessonEvaluation? Evaluation { get; set; }

        // 💰 المبالغ
        public decimal StudentHourlyRate { get; set; }
        public decimal TeacherHourlyRate { get; set; }
        public Currency Currency { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
