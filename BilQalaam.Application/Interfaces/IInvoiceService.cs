using BilQalaam.Application.DTOs.Invoices;
using BilQalaam.Application.Results;

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

        /// <summary>
        /// Get all lessons (invoices) »Ì‰  «—ÌŒÌ‰
        /// - SuperAdmin/Admin: Ì‘Ê›Ê« ﬂ· «·œ—Ê” ÊÌﬁœ—Ê Ì›· —Ê« » teacherId √Ê familyId
        /// - Admin: Ì‘Ê› ›ﬁÿ œ—Ê” «·„⁄·„Ì‰ «· «»⁄Ì‰ ·Â
        /// - Teacher: Ì‘Ê› ›ﬁÿ œ—Ê”Â ÊÌﬁœ— Ì›· — » familyId
        /// - Family: Ì‘Ê› ›ﬁÿ œ—Ê” ÿ·«»Â„ (»œÊ‰ ›· —)
        /// Default: „‰ √Ê· ÌÊ„ ›Ì «·‘Â— «·Õ«·Ì ··ÌÊ„
        /// userRole: · ÕœÌœ ≈–« ﬂ«‰ ÌÃ»  ÷„Ì‰ «”„ «·„⁄·„
        /// </summary>
        Task<Result<List<LessonInvoiceDto>>> GetAllLessonsInvoicesAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userRole = null);

        /// <summary>
        /// Get all lessons (invoices) with role-based filtering and teacher summary
        /// - SuperAdmin/Admin: Ì‘Ê›Ê« ﬂ· «·œ—Ê” ÊÌﬁœ—Ê Ì›· —Ê« » teacherId √Ê familyId
        /// - Admin: Ì‘Ê› ›ﬁÿ œ—Ê” «·„⁄·„Ì‰ «· «»⁄Ì‰ ·Â
        /// - Teacher: Ì‘Ê› ›ﬁÿ œ—Ê”Â ÊÌﬁœ— Ì›· — » familyId
        /// - Family: Ì‘Ê› ›ﬁÿ œ—Ê” ÿ·«»Â„ (»œÊ‰ ›· —)
        /// Default dates: from first day of current month to today
        /// </summary>
        Task<Result<LessonsInvoicesResponseDto>> GetLessonsInvoicesWithSummaryAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? teacherIdFilter = null,
            int? familyIdFilter = null,
            string? userRole = null,
            string? userId = null);
    }
}
