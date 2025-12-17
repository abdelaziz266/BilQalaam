using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Students
{
    public class StudentResponseDto
    {
        public int Id { get; set; }

        // ?? ÈíÇäÇÊ ÇáØÇáÈ
        public string StudentName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?????? ÇáÚÇÆáÉ ÇáÊÇÈÚ áåÇ
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        // ???? ÇáãÚáã ÇáÊÇÈÚ áå
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // ?? ÈíÇäÇÊ ÇáíæÒÑ
        public string UserId { get; set; } = string.Empty;

        // ?? ÇáÊæÇÑíÎ
        public DateTime CreatedAt { get; set; }
    }
}
