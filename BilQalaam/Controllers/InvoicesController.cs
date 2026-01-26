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

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }
        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }
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
