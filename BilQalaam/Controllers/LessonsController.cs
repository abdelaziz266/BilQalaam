using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Exceptions;
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
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly IUnitOfWork _unitOfWork;

        public LessonsController(ILessonService lessonService, IUnitOfWork unitOfWork)
        {
            _lessonService = lessonService;
            _unitOfWork = unitOfWork;
        }

        // 🔐 Helper methods
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }

        // ✅ GET: api/Lessons (الأدمن يشوف كل الدروس، المعلم يشوف دروسه بس)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            IEnumerable<LessonResponseDto> lessons;

            if (role == "SuperAdmin" || role == "Admin")
            {
                lessons = await _lessonService.GetAllAsync();
            }
            else if (role == "Teacher")
            {
                // جلب Teacher ID من User ID
                var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
                var teacher = teachers.FirstOrDefault();
                if (teacher == null)
                    return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(new List<LessonResponseDto>(), "No lessons found"));

                lessons = await _lessonService.GetByTeacherIdAsync(teacher.Id);
            }
            else
            {
                return Forbid();
            }

            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "Lessons retrieved successfully"));
        }

        // ✅ GET: api/Lessons/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lesson = await _lessonService.GetByIdAsync(id);
            return lesson == null
                ? NotFound(ApiResponseDto<LessonResponseDto>.Fail(new List<string> { "Lesson not found" }, "Not found", 404))
                : Ok(ApiResponseDto<LessonResponseDto>.Success(lesson, "Lesson retrieved successfully"));
        }

        // ✅ GET: api/Lessons/ByTeacher/{teacherId}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByTeacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(int teacherId)
        {
            var lessons = await _lessonService.GetByTeacherIdAsync(teacherId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "Lessons retrieved successfully"));
        }

        // ✅ GET: api/Lessons/ByFamily/{familyId}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByFamily/{familyId}")]
        public async Task<IActionResult> GetByFamilyId(int familyId)
        {
            var lessons = await _lessonService.GetByFamilyIdAsync(familyId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "Lessons retrieved successfully"));
        }

        // ✅ GET: api/Lessons/ByStudent/{studentId}
        [HttpGet("ByStudent/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var lessons = await _lessonService.GetByStudentIdAsync(studentId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "Lessons retrieved successfully"));
        }

        // ✅ GET: api/Lessons/ByDateRange?fromDate=...&toDate=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByDateRange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var lessons = await _lessonService.GetByDateRangeAsync(fromDate, toDate);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "Lessons retrieved successfully"));
        }

        // ✅ POST: api/Lessons (المعلم يسجل درس)
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
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
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                int teacherId;

                if (role == "Teacher")
                {
                    // المعلم يسجل درس لطلابه فقط
                    var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
                    var teacher = teachers.FirstOrDefault();
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<int>.Fail(new List<string> { "لم يتم العثور على بيانات المعلم" }, "Validation failed", 400));

                    teacherId = teacher.Id;
                }
                else
                {
                    // الأدمن يحتاج يحدد المعلم
                    var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);
                    if (student == null)
                        return BadRequest(ApiResponseDto<int>.Fail(new List<string> { "الطالب غير موجود" }, "Validation failed", 400));

                    teacherId = student.TeacherId;
                }

                var id = await _lessonService.CreateAsync(dto, teacherId, userId);
                return Ok(ApiResponseDto<int>.Success(id, "Lesson created successfully", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "Validation failed", 400));
            }
        }

        // ✅ PUT: api/Lessons/{id}
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonDto dto)
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
            var success = await _lessonService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Lesson updated successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Lesson not found" }, "Not found", 404));
        }

        // ✅ DELETE: api/Lessons/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _lessonService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Lesson deleted successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Lesson not found" }, "Not found", 404));
        }
    }
}
