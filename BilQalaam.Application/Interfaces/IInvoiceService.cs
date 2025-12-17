using BilQalaam.Application.DTOs.Invoices;

namespace BilQalaam.Application.Interfaces
{
    public interface IInvoiceService
    {
        // ?? ›« Ê—… «·⁄«∆·…
        Task<FamilyInvoiceDto?> GetFamilyInvoiceAsync(int familyId, DateTime fromDate, DateTime toDate);

        // ?? ›« Ê—… «·„⁄·„
        Task<TeacherInvoiceDto?> GetTeacherInvoiceAsync(int teacherId, DateTime fromDate, DateTime toDate);

        // ?? ›« Ê—… «·„‘—›
        Task<SupervisorInvoiceDto?> GetSupervisorInvoiceAsync(int supervisorId, DateTime fromDate, DateTime toDate);

        // ?? Ã·» ﬂ· «·›Ê« Ì— »«·›· — (··√œ„‰)
        Task<IEnumerable<FamilyInvoiceDto>> GetAllFamilyInvoicesAsync(DateTime fromDate, DateTime toDate, int? supervisorId = null);
        Task<IEnumerable<TeacherInvoiceDto>> GetAllTeacherInvoicesAsync(DateTime fromDate, DateTime toDate, int? supervisorId = null);
    }
}
