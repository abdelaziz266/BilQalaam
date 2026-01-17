using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class UpdateLessonDto
    {
        // 👨‍🎓 الطالب (لو عاوز يغير الطالب)
        public int? StudentId { get; set; }

        // 👨‍🏫 المعلم (مطلوب من Admin/SuperAdmin لتحديد المعلم عند التحديث)
        public int? TeacherId { get; set; }

        // 📅 بيانات الدرس
        public DateTime? LessonDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Notes { get; set; }

        // 🛑 الحضور
        public bool? IsAbsent { get; set; }

        // ⭐ تقييم الدرس
        public LessonEvaluation? Evaluation { get; set; }
    }
}
