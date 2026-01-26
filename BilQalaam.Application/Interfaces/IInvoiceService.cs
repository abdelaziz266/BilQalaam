using BilQalaam.Application.DTOs.Invoices;
using BilQalaam.Application.Results;

namespace BilQalaam.Application.Interfaces
{
    public interface IInvoiceService
    {
       
        Task<Result<LessonsInvoicesResponseDto>> GetLessonsInvoicesWithSummaryAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? teacherIdFilter = null,
            int? familyIdFilter = null,
            string? userRole = null,
            string? userId = null);
    }
}
