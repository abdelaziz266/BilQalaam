namespace BilQalaam.Application.DTOs.Invoices
{
    public class InvoiceFilterDto
    {
        // ?? ›· —… »«· «—ÌŒ
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // ?? ›· —… Õ”» «·‰Ê⁄
        public int? FamilyId { get; set; }
        public int? TeacherId { get; set; }
        public int? StudentId { get; set; }
        public int? SupervisorId { get; set; }

        // ?? ‰Ê⁄ «·›« Ê—… (Family, Teacher, Supervisor)
        public string? InvoiceType { get; set; }
    }
}
