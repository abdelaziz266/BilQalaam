using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.DTOs.Teachers;

namespace BilQalaam.Application.DTOs.Invoices
{
    public class LessonsInvoicesResponseDto
    {
        // ?? ŞÇÆãÉ ÇáÏÑæÓ
        public List<LessonInvoiceDto> Lessons { get; set; } = new();

        // ???? ÈíÇäÇÊ ÇáãÚáã (ÅĞÇ Êã ÇáİáÊÑÉ ÈãÚáã ãÚíä Ãæ ÅĞÇ ßÇä ÇáãÓÊÎÏã ãÚáã)
        public TeacherSummaryDto? TeacherSummary { get; set; }

        public FamilySummaryDto? FamilySummary { get; set; }

        // ?????? ??????? SuperAdmin
        public SuperAdminSummaryDto? SuperAdminSummary { get; set; }

        // ?? ÅÍÕÇÆíÇÊ ÚÇãÉ
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TeacherSummaryDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ ÈÊÇÚ ÇáãÚáã
        public decimal HourlyRate { get; set; }
        public string Currency { get; set; } = string.Empty;

        // ?? ÅÌãÇáí ÚÏÏ ÓÇÚÇÊ ÇáãÚáã
        public decimal TotalHours { get; set; }

        // ?? ÅÌãÇáí İáæÓ ÇáãÚáã
        public decimal TotalEarnings { get; set; }
    }

    public class FamilySummaryDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class SuperAdminSummaryDto
    {
        // ?? ÅÌãÇáí ÚÏÏ ÇáÓÇÚÇÊ
        public decimal TotalHours { get; set; }

        // ?? ÅÌãÇáí ÇáãÈáÛ ÈÇáÏæáÇÑ ÇáÃãÑíßí (USD)
        public decimal TotalAmountUSD { get; set; }

        // ?? ÅÌãÇáí ÇáãÈáÛ ÈÇáÌäíå ÇáãÕÑí (EGP)
        public decimal TotalAmountEGP { get; set; }
    }
}
