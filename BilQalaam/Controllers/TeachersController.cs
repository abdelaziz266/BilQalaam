using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // ?? Helper method ··Õ’Ê· ⁄·Ï User ID «·Õ«·Ì
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        // ? GET: api/Teachers/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (teachers, totalCount) = await _teacherService.GetAllAsync(pageNumber, pageSize);
            var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedResponse = new PaginatedResponseDto<TeacherResponseDto>
            {
                Items = teachers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                PagesCount = pagesCount
            };

            return Ok(ApiResponseDto<PaginatedResponseDto<TeacherResponseDto>>.Success(
                paginatedResponse,
                " „ «” —Ã«⁄ «·„⁄·„Ì‰ »‰Ã«Õ"
            ));
        }

        // ? GET: api/Teachers/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            return teacher == null
                ? NotFound(ApiResponseDto<TeacherResponseDto>.Fail(
                    new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<TeacherResponseDto>.Success(
                    teacher,
                    " „ «” —Ã«⁄ «·„⁄·„ »‰Ã«Õ"
                ));
        }

        // ? POST: api/Teachers/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
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
                var id = await _teacherService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, " „ ≈‰‘«¡ «·„⁄·„ »‰Ã«Õ", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }
        }

        // ? PUT: api/Teachers/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
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
            var success = await _teacherService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, " „  ÕœÌÀ «·„⁄·„ »‰Ã«Õ"))
                : NotFound(ApiResponseDto<bool>.Fail(
                    new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ));
        }

        // ? DELETE: api/Teachers/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _teacherService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, " „ Õ–› «·„⁄·„ »‰Ã«Õ"))
                : NotFound(ApiResponseDto<bool>.Fail(
                    new List<string> { "«·„⁄·„ €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ));
        }
    }
}
