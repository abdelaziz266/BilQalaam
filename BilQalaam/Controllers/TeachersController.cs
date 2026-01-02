using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Teachers;
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

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        [HttpGet("get")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _teacherService.GetAllAsync(pageNumber, pageSize, GetCurrentUserRole(), GetCurrentUserId(), searchText);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<TeacherResponseDto>>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáãÚáãíä ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<TeacherResponseDto>>.Fail(result.Errors, "İÔá İí ÌáÈ ÇáãÚáãíä", 400));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _teacherService.GetByIdAsync(id, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<TeacherResponseDto>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáãÚáã ÈäÌÇÍ"))
                : NotFound(ApiResponseDto<TeacherResponseDto>.Fail(result.Errors, "áã íÊã ÇáÚËæÑ Úáíå", 404));
        }

        /// <summary>
        /// Get teachers by family ID (same supervisor)
        /// </summary>
        [HttpGet("by-family/{familyId}")]
        public async Task<IActionResult> GetByFamilyId(int familyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10000)
        {
            
            var result = await _teacherService.GetByFamilyIdAsync(familyId, pageNumber, pageSize);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<TeacherResponseDto>>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáãÚáãíä ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<TeacherResponseDto>>.Fail(result.Errors, "İÔá İí ÌáÈ ÇáãÚáãíä", 400));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<int>.Fail(errors, "İÔá ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ", 400));
            }

            var result = await _teacherService.CreateAsync(dto, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<int>.Success(result.Data, "Êã ÅäÔÇÁ ÇáãÚáã ÈäÌÇÍ", 201))
                : BadRequest(ApiResponseDto<int>.Fail(result.Errors, "İÔá İí ÅäÔÇÁ ÇáãÚáã", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<bool>.Fail(errors, "İÔá ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ", 400));
            }

            var result = await _teacherService.UpdateAsync(id, dto, GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "Êã ÊÍÏíË ÇáãÚáã ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "İÔá İí ÊÍÏíË ÇáãÚáã", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _teacherService.DeleteAsync(id);

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "Êã ÍĞİ ÇáãÚáã ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "İÔá İí ÍĞİ ÇáãÚáã", 400));
        }

        /// <summary>
        /// Get teacher details with related families and students
        /// Accepts single or multiple teacher IDs (required at least one)
        /// </summary>
        [HttpGet("details")]
        public async Task<IActionResult> GetTeacherDetails(
            [FromQuery(Name = "ids")] IEnumerable<int>? ids = null)
        {
            // ÇáÊÍŞŞ ãä Ãä åäÇß ãÚÑİ æÇÍÏ Úáì ÇáÃŞá
            if (ids == null || !ids.Any())
            {
                return BadRequest(ApiResponseDto<object>.Fail(
                    new List<string> { "íÌÈ ÊÍÏíÏ ãÚÑİ æÇÍÏ Úáì ÇáÃŞá" },
                    "ÈíÇäÇÊ ãİŞæÏÉ",
                    400
                ));
            }

            var distinctIds = ids.Distinct().ToList();

            // áæ ÈÚÊ single ID
            if (distinctIds.Count == 1)
            {
                var singleId = distinctIds.First();
                var result = await _teacherService.GetTeacherDetailsAsync(singleId);
                return result.IsSuccess
                    ? Ok(ApiResponseDto<TeacherDetailsDto>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÊİÇÕíá ÇáãÚáã ÈäÌÇÍ"))
                    : NotFound(ApiResponseDto<TeacherDetailsDto>.Fail(result.Errors, "áã íÊã ÇáÚËæÑ Úáíå", 404));
            }

            // áæ ÈÚÊ ÚÏÉ IDs
            var multipleResult = await _teacherService.GetMultipleTeachersDetailsAsync(distinctIds);
            return multipleResult.IsSuccess
                ? Ok(ApiResponseDto<List<TeacherDetailsDto>>.Success(multipleResult.Data!, "Êã ÇÓÊÑÌÇÚ ÊİÇÕíá ÇáãÚáãíä ÈäÌÇÍ"))
                : NotFound(ApiResponseDto<List<TeacherDetailsDto>>.Fail(multipleResult.Errors, "İÔá İí ÌáÈ ÇáÈíÇäÇÊ", 404));
        }
    }
}
