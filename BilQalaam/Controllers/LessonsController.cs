using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        // ✅ GET: api/Lessons/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] IEnumerable<int>? supervisorIds = null,
            [FromQuery] IEnumerable<int>? teacherIds = null,
            [FromQuery] IEnumerable<int>? studentIds = null,
            [FromQuery] IEnumerable<int>? familyIds = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _lessonService.GetAllAsync(
                pageNumber, pageSize,
                supervisorIds?.Distinct(),
                teacherIds?.Distinct(),
                studentIds?.Distinct(),
                familyIds?.Distinct(),
                fromDate, toDate,
                GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<LessonPaginatedResponseDto>.Success(result.Data!, "تم استرجاع الدروس بنجاح"))
                : BadRequest(ApiResponseDto<LessonPaginatedResponseDto>.Fail(result.Errors, "فشل في جلب الدروس", 400));
        }

        // ✅ GET: api/Lessons/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _lessonService.GetByIdAsync(id, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<LessonResponseDto>.Success(result.Data!, "تم استرجاع الدرس بنجاح"))
                : NotFound(ApiResponseDto<LessonResponseDto>.Fail(result.Errors, "لم يتم العثور عليه", 404));
        }

        // ✅ POST: api/Lessons/create
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<int>.Fail(errors, "فشل التحقق من البيانات", 400));
            }

            var result = await _lessonService.CreateAsync(dto, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<int>.Success(result.Data, "تم إنشاء الدرس بنجاح", 201))
                : BadRequest(ApiResponseDto<int>.Fail(result.Errors, "فشل في إنشاء الدرس", 400));
        }

        // ✅ PUT: api/Lessons/update/{id}
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<bool>.Fail(errors, "فشل التحقق من البيانات", 400));
            }

            var result = await _lessonService.UpdateAsync(id, dto, GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "تم تحديث الدرس بنجاح"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "فشل في تحديث الدرس", 400));
        }

        // ✅ DELETE: api/Lessons/delete/{id}
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _lessonService.DeleteAsync(id);

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "تم حذف الدرس بنجاح"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "فشل في حذف الدرس", 400));
        }
    }
}
