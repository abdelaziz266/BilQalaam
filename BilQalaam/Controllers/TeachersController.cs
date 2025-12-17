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

        // ? GET: api/Teachers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teachers = await _teacherService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<TeacherResponseDto>>.Success(teachers, "Teachers retrieved successfully"));
        }

        // ? GET: api/Teachers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            return teacher == null
                ? NotFound(ApiResponseDto<TeacherResponseDto>.Fail(new List<string> { "Teacher not found" }, "Not found", 404))
                : Ok(ApiResponseDto<TeacherResponseDto>.Success(teacher, "Teacher retrieved successfully"));
        }

        // ? POST: api/Teachers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<int>.Fail(modelErrors, "Validation failed", 400));
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var id = await _teacherService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, "Teacher created successfully", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "Validation failed", 400));
            }
        }

        // ? PUT: api/Teachers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<bool>.Fail(modelErrors, "Validation failed", 400));
            }

            var currentUserId = GetCurrentUserId();
            var success = await _teacherService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Teacher updated successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Teacher not found" }, "Not found", 404));
        }

        // ? DELETE: api/Teachers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _teacherService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Teacher deleted successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Teacher not found" }, "Not found", 404));
        }
    }
}
