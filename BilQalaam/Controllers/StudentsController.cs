using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        [HttpGet("get")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] IEnumerable<int>? familyIds = null,
            [FromQuery] IEnumerable<int>? teacherIds = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _studentService.GetAllAsync(
                pageNumber, pageSize,
                familyIds?.Distinct(),
                teacherIds?.Distinct(),
                GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Success(result.Data!, " „ «” —Ã«⁄ «·ÿ·«» »‰Ã«Õ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Fail(result.Errors, "›‘· ›Ì Ã·» «·ÿ·«»", 400));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _studentService.GetByIdAsync(id, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<StudentResponseDto>.Success(result.Data!, " „ «” —Ã«⁄ «·ÿ«·» »‰Ã«Õ"))
                : NotFound(ApiResponseDto<StudentResponseDto>.Fail(result.Errors, "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ", 404));
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<int>.Fail(errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }

            var result = await _studentService.CreateAsync(dto, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<int>.Success(result.Data, " „ ≈‰‘«¡ «·ÿ«·» »‰Ã«Õ", 201))
                : BadRequest(ApiResponseDto<int>.Fail(result.Errors, "›‘· ›Ì ≈‰‘«¡ «·ÿ«·»", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<bool>.Fail(errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }

            var result = await _studentService.UpdateAsync(id, dto, GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, " „  ÕœÌÀ «·ÿ«·» »‰Ã«Õ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· ›Ì  ÕœÌÀ «·ÿ«·»", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _studentService.DeleteAsync(id);

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, " „ Õ–› «·ÿ«·» »‰Ã«Õ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· ›Ì Õ–› «·ÿ«·»", 400));
        }
        [HttpGet("by-teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(int teacherId)
        {
             var result = await _studentService.GetByTeacherIdAsync(teacherId);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Success(result.Data!, " „ «” —Ã«⁄ «·ÿ·«» »‰Ã«Õ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Fail(result.Errors, "›‘· ›Ì Ã·» «·ÿ·«»", 400));
        }
    }
}
