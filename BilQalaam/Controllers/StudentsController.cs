using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Students;
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

        // ? GET: api/Students
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<StudentResponseDto>>.Success(students, "Students retrieved successfully"));
        }

        // ? GET: api/Students/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            return student == null
                ? NotFound(ApiResponseDto<StudentResponseDto>.Fail(new List<string> { "Student not found" }, "Not found", 404))
                : Ok(ApiResponseDto<StudentResponseDto>.Success(student, "Student retrieved successfully"));
        }

        // ? GET: api/Students/ByFamily/{familyId}
        [HttpGet("ByFamily/{familyId}")]
        public async Task<IActionResult> GetByFamilyId(int familyId)
        {
            var students = await _studentService.GetByFamilyIdAsync(familyId);
            return Ok(ApiResponseDto<IEnumerable<StudentResponseDto>>.Success(students, "Students retrieved successfully"));
        }

        // ? GET: api/Students/ByTeacher/{teacherId}
        [HttpGet("ByTeacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(int teacherId)
        {
            var students = await _studentService.GetByTeacherIdAsync(teacherId);
            return Ok(ApiResponseDto<IEnumerable<StudentResponseDto>>.Success(students, "Students retrieved successfully"));
        }

        // ? POST: api/Students
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
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
                var id = await _studentService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, "Student created successfully", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "Validation failed", 400));
            }
        }

        // ? PUT: api/Students/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
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
            var success = await _studentService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Student updated successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Student not found" }, "Not found", 404));
        }

        // ? DELETE: api/Students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _studentService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Student deleted successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Student not found" }, "Not found", 404));
        }
    }
}
