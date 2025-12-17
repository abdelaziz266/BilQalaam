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
                ? NotFound(ApiResponseDto<FamilyInvoiceDto>.Fail(new List<string> { "Family not found" }, "Not found", 404))
                : Ok(ApiResponseDto<FamilyInvoiceDto>.Success(invoice, "Family invoice retrieved successfully"));
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
                ? NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(new List<string> { "Teacher not found" }, "Not found", 404))
                : Ok(ApiResponseDto<TeacherInvoiceDto>.Success(invoice, "Teacher invoice retrieved successfully"));
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
                return NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(new List<string> { "Teacher not found" }, "Not found", 404));

            var invoice = await _invoiceService.GetTeacherInvoiceAsync(teacher.Id, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<TeacherInvoiceDto>.Fail(new List<string> { "No lessons found" }, "Not found", 404))
                : Ok(ApiResponseDto<TeacherInvoiceDto>.Success(invoice, "Teacher invoice retrieved successfully"));
        }

        // ? GET: api/Invoices/Supervisor/{supervisorId}?fromDate=...&toDate=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("Supervisor/{supervisorId}")]
        public async Task<IActionResult> GetSupervisorInvoice(int supervisorId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var invoice = await _invoiceService.GetSupervisorInvoiceAsync(supervisorId, fromDate, toDate);
            return invoice == null
                ? NotFound(ApiResponseDto<SupervisorInvoiceDto>.Fail(new List<string> { "Supervisor not found" }, "Not found", 404))
                : Ok(ApiResponseDto<SupervisorInvoiceDto>.Success(invoice, "Supervisor invoice retrieved successfully"));
        }

        // ? GET: api/Invoices/AllFamilies?fromDate=...&toDate=...&supervisorId=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("AllFamilies")]
        public async Task<IActionResult> GetAllFamilyInvoices([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? supervisorId = null)
        {
            var invoices = await _invoiceService.GetAllFamilyInvoicesAsync(fromDate, toDate, supervisorId);
            return Ok(ApiResponseDto<IEnumerable<FamilyInvoiceDto>>.Success(invoices, "Family invoices retrieved successfully"));
        }

        // ? GET: api/Invoices/AllTeachers?fromDate=...&toDate=...&supervisorId=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("AllTeachers")]
        public async Task<IActionResult> GetAllTeacherInvoices([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? supervisorId = null)
        {
            var invoices = await _invoiceService.GetAllTeacherInvoicesAsync(fromDate, toDate, supervisorId);
            return Ok(ApiResponseDto<IEnumerable<TeacherInvoiceDto>>.Success(invoices, "Teacher invoices retrieved successfully"));
        }
    }
}
