using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Invoices
{
    // ?? İÇÊæÑÉ ÇáãÔÑİ (ãáÎÕ ßá ÇáãÚáãíä æÇáÚÇÆáÇÊ ÇáÊÇÈÚíä áå)
    public class SupervisorInvoiceDto
    {
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;

        // ?? İÊÑÉ ÇáİÇÊæÑÉ
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // ?? ãáÎÕ ÚÇã
        public int TotalFamilies { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalFamilyAmount { get; set; }  // ÅÌãÇáí ÇáãØáæÈ ãä ÇáÚÇÆáÇÊ
        public decimal TotalTeacherEarnings { get; set; }  // ÅÌãÇáí ÃÌæÑ ÇáãÚáãíä
        public Currency Currency { get; set; }

        // ?? ÊİÇÕíá ÇáÚÇÆáÇÊ
        public List<SupervisorFamilyDetailDto> Families { get; set; } = new();

        // ?? ÊİÇÕíá ÇáãÚáãíä
        public List<SupervisorTeacherDetailDto> Teachers { get; set; } = new();
    }

    public class SupervisorFamilyDetailDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SupervisorTeacherDetailDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalEarnings { get; set; }
    }
}
