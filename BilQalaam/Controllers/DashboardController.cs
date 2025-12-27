using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Dashboard;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _dashboardService.GetDashboardAsync(GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<DashboardDto>.Success(result.Data!, " „ «” —Ã«⁄ »Ì«‰«  ·ÊÕ… «·»Ì«‰«  »‰Ã«Õ"))
                : BadRequest(ApiResponseDto<DashboardDto>.Fail(result.Errors, "›‘· ›Ì Ã·» «·»Ì«‰« ", 400));
        }
    }
}
