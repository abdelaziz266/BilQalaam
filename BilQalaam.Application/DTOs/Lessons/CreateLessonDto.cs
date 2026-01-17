using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class CreateLessonDto
    {
        // 👨‍🎓 الطالب (ID من جدول Students)
        public int StudentId { get; set; }

        // 👨‍🏫 المعلم المستهدف (يُطلب من الـ Admin أو SuperAdmin فقط)
        public int? TeacherId { get; set; }

        // 📅 بيانات الدرس
        public DateTime LessonDate { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }

        // 🛑 الحضور
        public bool IsAbsent { get; set; } = false;

        // ⭐ تقييم الدرس
        public LessonEvaluation? Evaluation { get; set; }
    }
}
