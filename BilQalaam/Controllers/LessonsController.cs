using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Exceptions;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

            var role = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            var result = await _lessonService.GetAllAsync(
                pageNumber,
                pageSize,
                supervisorIds?.Distinct(),
                teacherIds?.Distinct(),
                studentIds?.Distinct(),
                familyIds?.Distinct(),
                fromDate,
                toDate,
                role,
                userId
            );

            return Ok(ApiResponseDto<PaginatedResponseDto<LessonResponseDto>>.Success(
                result,
                "تم استرجاع الدروس بنجاح"
            ));
        }

        // ✅ GET: api/Lessons/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lesson = await _lessonService.GetByIdAsync(id);
            return lesson == null
                ? NotFound(ApiResponseDto<LessonResponseDto>.Fail(
                    new List<string> { "الدرس غير موجود" },
                    "لم يتم العثور عليه",
                    404
                ))
                : Ok(ApiResponseDto<LessonResponseDto>.Success(
                    lesson,
                    "تم استرجاع الدرس بنجاح"
                ));
        }

        // ✅ GET: api/Lessons/ByTeacher/{teacherId}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByTeacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(int teacherId)
        {
            var (lessons, _) = await _lessonService.GetByTeacherIdAsync(teacherId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "تم استرجاع الدروس بنجاح"));
        }

        // ✅ GET: api/Lessons/ByFamily/{familyId}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByFamily/{familyId}")]
        public async Task<IActionResult> GetByFamilyId(int familyId)
        {
            var lessons = await _lessonService.GetByFamilyIdAsync(familyId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "تم استرجاع الدروس بنجاح"));
        }

        // ✅ GET: api/Lessons/ByStudent/{studentId}
        [HttpGet("ByStudent/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var lessons = await _lessonService.GetByStudentIdAsync(studentId);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "تم استرجاع الدروس بنجاح"));
        }

        // ✅ GET: api/Lessons/ByDateRange?fromDate=...&toDate=...
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("ByDateRange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var lessons = await _lessonService.GetByDateRangeAsync(fromDate, toDate);
            return Ok(ApiResponseDto<IEnumerable<LessonResponseDto>>.Success(lessons, "تم استرجاع الدروس بنجاح"));
        }

        // ✅ POST: api/Lessons/create
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<int>.Fail(modelErrors, "فشل التحقق من البيانات", 400));
            }

            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);
                if (student == null)
                    return BadRequest(ApiResponseDto<int>.Fail(
                        new List<string> { "الطالب غير موجود" },
                        "فشل التحقق من البيانات",
                        400
                    ));

                int teacherId;
                int? supervisorId = null;

                if (role == "Teacher")
                {
                    var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
                    var teacher = teachers.FirstOrDefault();
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "لم يتم العثور على بيانات المعلم" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = teacher.Id;
                    supervisorId = teacher.SupervisorId;
                }
                else if (role == "Admin")
                {
                    if (!dto.TeacherId.HasValue)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "المعلم مطلوب" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = dto.TeacherId.Value;
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "المعلم غير موجود" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
                    var supervisor = supervisors.FirstOrDefault();
                    if (supervisor == null)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "لم يتم العثور على بيانات المشرف" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    supervisorId = supervisor.Id;
                }
                else // SuperAdmin
                {
                    if (!dto.TeacherId.HasValue)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "المعلم مطلوب" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = dto.TeacherId.Value;
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<int>.Fail(
                            new List<string> { "المعلم غير موجود" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    supervisorId = teacher.SupervisorId;
                }

                var id = await _lessonService.CreateAsync(dto, teacherId, userId, supervisorId);
                return Ok(ApiResponseDto<int>.Success(id, "تم إنشاء الدرس بنجاح", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "فشل التحقق من البيانات", 400));
            }
        }

        // ✅ PUT: api/Lessons/update/{id}
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<bool>.Fail(modelErrors, "فشل التحقق من البيانات", 400));
            }

            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                int teacherId;
                int? supervisorId = null;

                if (role == "Teacher")
                {
                    var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
                    var teacher = teachers.FirstOrDefault();
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "لم يتم العثور على بيانات المعلم" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = teacher.Id;
                    supervisorId = teacher.SupervisorId;
                }
                else if (role == "Admin")
                {
                    if (!dto.TeacherId.HasValue)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "المعلم مطلوب" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = dto.TeacherId.Value;
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "المعلم غير موجود" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
                    var supervisor = supervisors.FirstOrDefault();
                    if (supervisor == null)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "لم يتم العثور على بيانات المشرف" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    supervisorId = supervisor.Id;
                }
                else // SuperAdmin
                {
                    if (!dto.TeacherId.HasValue)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "المعلم مطلوب" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    teacherId = dto.TeacherId.Value;
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                    if (teacher == null)
                        return BadRequest(ApiResponseDto<bool>.Fail(
                            new List<string> { "المعلم غير موجود" },
                            "فشل التحقق من البيانات",
                            400
                        ));

                    supervisorId = teacher.SupervisorId;
                }

                var success = await _lessonService.UpdateAsync(id, dto, userId, teacherId, supervisorId);
                return success
                    ? Ok(ApiResponseDto<bool>.Success(true, "تم تحديث الدرس بنجاح"))
                    : NotFound(ApiResponseDto<bool>.Fail(
                        new List<string> { "الدرس غير موجود" },
                        "لم يتم العثور عليه",
                        404
                    ));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<bool>.Fail(ex.Errors, "فشل التحقق من البيانات", 400));
            }
        }

        // ✅ DELETE: api/Lessons/delete/{id}
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _lessonService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "تم حذف الدرس بنجاح"))
                : NotFound(ApiResponseDto<bool>.Fail(
                    new List<string> { "الدرس غير موجود" },
                    "لم يتم العثور عليه",
                    404
                ));
        }
    }
}
