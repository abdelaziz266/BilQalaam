using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Invoices;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUnitOfWork _unitOfWork;

        public InvoicesController(IInvoiceService invoiceService, IUnitOfWork unitOfWork)
        {
            _invoiceService = invoiceService;
            _unitOfWork = unitOfWork;
        }

        // ?? Helper methods
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }

        // ? GET: api/Invoices/Family/{familyId}?fromDate=...&toDate=...
        [HttpGet("Family/{familyId}")]
        public async Task<IActionResult> GetFamilyInvoice(int familyId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var invoice = await _invoiceService.GetFamilyInvoiceAsync(familyId, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<FamilyInvoiceDto>.Fail(
                    new List<string> { "«·⁄«∆·… €Ì— „ÊÃÊœ…" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ«",
                    404
                ))
                : Ok(ApiResponseDto<FamilyInvoiceDto>.Success(
                    invoice,
                    " „ «” —Ã«⁄ ›« Ê—… «·⁄«∆·… »‰Ã«Õ"
                ));
        }

        // ? GET: api/Invoices/Teacher/{teacherId}?fromDate=...&toDate=...
        [HttpGet("Teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherInvoice(int teacherId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var role = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // «·„⁄·„ Ì‘Ê› ›« Ê— Â »”
            if (role == "Teacher")
            {
                var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
                var teacher = teachers.FirstOrDefault();
                if (teacher == null || teacher.Id != teacherId)
                    return Forbid();
            }

            var invoice = await _invoiceService.GetTeacherInvoiceAsync(teacherId, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(
                    new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<TeacherInvoiceDto>.Success(
                    invoice,
                    " „ «” —Ã«⁄ ›« Ê—… «·„⁄·„ »‰Ã«Õ"
                ));
        }

        // ? GET: api/Invoices/Teacher/My?fromDate=...&toDate=... («·„⁄·„ Ì‘Ê› ›« Ê— Â)
        [Authorize(Roles = "Teacher")]
        [HttpGet("Teacher/My")]
        public async Task<IActionResult> GetMyTeacherInvoice([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var userId = GetCurrentUserId();
            var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
            var teacher = teachers.FirstOrDefault();

            if (teacher == null)
                return NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(
                    new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ));

            var invoice = await _invoiceService.GetTeacherInvoiceAsync(teacher.Id, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(
                    new List<string> { "·„ Ì „ «·⁄ÀÊ— ⁄·Ï œ—Ê”" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<TeacherInvoiceDto>.Success(
                    invoice,
                    " „ «” —Ã«⁄ ›« Ê— ﬂ »‰Ã«Õ"
                ));
        }

        // ? GET: api/Invoices/Supervisor/{supervisorId}?fromDate=...&toDate=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("Supervisor/{supervisorId}")]
        public async Task<IActionResult> GetSupervisorInvoice(int supervisorId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var invoice = await _invoiceService.GetSupervisorInvoiceAsync(supervisorId, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<SupervisorInvoiceDto>.Fail(
                    new List<string> { "«·„‘—› €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<SupervisorInvoiceDto>.Success(
                    invoice,
                    " „ «” —Ã«⁄ ›« Ê—… «·„‘—› »‰Ã«Õ"
                ));
        }

        // ? GET: api/Invoices/AllFamilies?fromDate=...&toDate=...&supervisorId=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("AllFamilies")]
        public async Task<IActionResult> GetAllFamilyInvoices([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? supervisorId = null)
        {
            var invoices = await _invoiceService.GetAllFamilyInvoicesAsync(fromDate, toDate, supervisorId);
            return Ok(ApiResponseDto<IEnumerable<FamilyInvoiceDto>>.Success(
                invoices,
                " „ «” —Ã«⁄ ›Ê« Ì— «·⁄«∆·«  »‰Ã«Õ"
            ));
        }

        // ? GET: api/Invoices/AllTeachers?fromDate=...&toDate=...&supervisorId=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("AllTeachers")]
        public async Task<IActionResult> GetAllTeacherInvoices([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? supervisorId = null)
        {
            var invoices = await _invoiceService.GetAllTeacherInvoicesAsync(fromDate, toDate, supervisorId);
            return Ok(ApiResponseDto<IEnumerable<TeacherInvoiceDto>>.Success(
                invoices,
                " „ «” —Ã«⁄ ›Ê« Ì— «·„⁄·„Ì‰ »‰Ã«Õ"
            ));
        }

        // ? GET: api/Invoices/Lessons?fromDate=...&toDate=...
        /// <summary>
        /// Get all lessons (invoices) between date range
        /// Default: from first day of current month to today
        /// Teacher name only shown for Admin/SuperAdmin
        /// </summary>
        [HttpGet("Lessons")]
        public async Task<IActionResult> GetAllLessonsInvoices([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userRole = GetCurrentUserRole();
            var result = await _invoiceService.GetAllLessonsInvoicesAsync(fromDate, toDate, userRole);

            return result.IsSuccess
                ? Ok(ApiResponseDto<List<LessonInvoiceDto>>.Success(
                    result.Data!,
                    " „ Ã·» «·œ—Ê” »‰Ã«Õ"
                ))
                : BadRequest(ApiResponseDto<List<LessonInvoiceDto>>.Fail(
                    result.Errors,
                    "›‘· ›Ì Ã·» «·œ—Ê”",
                    400
                ));
        }

        // ? GET: api/Invoices/LessonsWithSummary?fromDate=...&toDate=...&teacherIdFilter=...&familyIdFilter=...
        /// <summary>
        /// Get all lessons with teacher summary and role-based filtering
        /// SuperAdmin/Admin: Ì‘Ê›Ê« ﬂ· «·œ—Ê” ÊÌﬁœ—Ê Ì›· —Ê« » teacherId √Ê familyId
        /// Admin: Ì‘Ê› ›ﬁÿ œ—Ê” «·„⁄·„Ì‰ «· «»⁄Ì‰ ·Â
        /// Teacher: Ì‘Ê› ›ﬁÿ œ—Ê”Â ÊÌﬁœ— Ì›· — » familyId
        /// Family: Ì‘Ê› ›ﬁÿ œ—Ê” ÿ·«»Â„
        /// </summary>
        [HttpGet("LessonsWithSummary")]
        public async Task<IActionResult> GetLessonsInvoicesWithSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? teacherIdFilter = null,
            [FromQuery] int? familyIdFilter = null)
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            
            var result = await _invoiceService.GetLessonsInvoicesWithSummaryAsync(
                fromDate,
                toDate,
                teacherIdFilter,
                familyIdFilter,
                userRole,
                userId);

            return result.IsSuccess
                ? Ok(ApiResponseDto<LessonsInvoicesResponseDto>.Success(
                    result.Data!,
                    " „ Ã·» «·œ—Ê” »‰Ã«Õ"
                ))
                : BadRequest(ApiResponseDto<LessonsInvoicesResponseDto>.Fail(
                    result.Errors,
                    "›‘· ›Ì Ã·» «·œ—Ê”",
                    400
                ));
        }
    }
}
