using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Students
{
    public class CreateStudentDto
    {
        // ?? ÈíÇäÇÊ ÇáíæÒÑ ÇáÌÏíÏ
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // ?? ÈíÇäÇÊ ÇáØÇáÈ
        public string StudentName { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?????? ÇáÚÇÆáÉ ÇáÊÇÈÚ áåÇ (ID ãä ÌÏæá Families)
        public int FamilyId { get; set; }

        // ???? ÇáãÚáã ÇáÊÇÈÚ áå (ID ãä ÌÏæá Teachers)
        public int TeacherId { get; set; }
    }
}
