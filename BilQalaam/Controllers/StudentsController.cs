using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // ?? Helper method ··Õ’Ê· ⁄·Ï User ID «·Õ«·Ì
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        // ? GET: api/Students/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] IEnumerable<int>? familyIds = null,
        [FromQuery] IEnumerable<int>? teacherIds = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (students, totalCount) =
                await _studentService.GetAllAsync(
                    pageNumber,
                    pageSize,
                    familyIds?.Distinct(),
                    teacherIds?.Distinct()
                );

            var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Success(
                new PaginatedResponseDto<StudentResponseDto>
                {
                    Items = students,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                },
                " „ «” —Ã«⁄ «·ÿ·«» »‰Ã«Õ"
            ));
        }

        // ? GET: api/Students/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            return student == null
                ? NotFound(ApiResponseDto<StudentResponseDto>.Fail(
                    new List<string> { "«·ÿ«·» €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<StudentResponseDto>.Success(
                    student,
                    " „ «” —Ã«⁄ «·ÿ«·» »‰Ã«Õ"
                ));
        }

        // ? POST: api/Students/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<int>.Fail(modelErrors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var id = await _studentService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, " „ ≈‰‘«¡ «·ÿ«·» »‰Ã«Õ", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }
        }

        // ? PUT: api/Students/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<bool>.Fail(modelErrors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }

            var currentUserId = GetCurrentUserId();
            var success = await _studentService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, " „  ÕœÌÀ «·ÿ«·» »‰Ã«Õ"))
                : NotFound(ApiResponseDto<bool>.Fail(
                    new List<string> { "«·ÿ«·» €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ));
        }

        // ? DELETE: api/Students/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _studentService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, " „ Õ–› «·ÿ«·» »‰Ã«Õ"))
                : NotFound(ApiResponseDto<bool>.Fail(
                    new List<string> { "«·ÿ«·» €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ));
        }
        [HttpGet("by-families")]
        public async Task<IActionResult> GetByFamilies(
            [FromQuery] IEnumerable<int> familyIds,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10000)
        {
            if (!familyIds.Any())
                return BadRequest("FamilyIds is required");

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (students, totalCount) =
                await _studentService.GetByFamilyIdsAsync(
                    familyIds.Distinct(),
                    pageNumber,
                    pageSize);

            var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(ApiResponseDto<PaginatedResponseDto<StudentResponseDto>>.Success(
                new PaginatedResponseDto<StudentResponseDto>
                {
                    Items = students,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                },
                " „ «” —Ã«⁄ ÿ·«» «·⁄«∆·«  »‰Ã«Õ"
            ));
        }
    }
}
