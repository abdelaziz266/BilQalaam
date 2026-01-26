using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class FamiliesController : ControllerBase
    {
        private readonly IFamilyService _familyService;

        public FamiliesController(IFamilyService familyService)
        {
            _familyService = familyService;
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

            var result = await _familyService.GetAllAsync(pageNumber, pageSize, GetCurrentUserRole(), GetCurrentUserId(), searchText);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<FamilyResponseDto>>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáÚÇÆáÇÊ ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<FamilyResponseDto>>.Fail(result.Errors, "İÔá İí ÌáÈ ÇáÚÇÆáÇÊ", 400));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _familyService.GetByIdAsync(id, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<FamilyResponseDto>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáÚÇÆáÉ ÈäÌÇÍ"))
                : NotFound(ApiResponseDto<FamilyResponseDto>.Fail(result.Errors, "áã íÊã ÇáÚËæÑ ÚáíåÇ", 404));
        }

        [HttpGet("by-teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacherId(int teacherId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _familyService.GetByTeacherIdAsync(teacherId, pageNumber, pageSize);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<FamilyResponseDto>>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÇáÚÇÆáÇÊ ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<FamilyResponseDto>>.Fail(result.Errors, "İÔá İí ÌáÈ ÇáÚÇÆáÇÊ", 400));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<int>.Fail(errors, "İÔá ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ", 400));
            }

            var result = await _familyService.CreateAsync(dto, GetCurrentUserRole(), GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<int>.Success(result.Data, "Êã ÅäÔÇÁ ÇáÚÇÆáÉ ÈäÌÇÍ", 201))
                : BadRequest(ApiResponseDto<int>.Fail(result.Errors, "İÔá İí ÅäÔÇÁ ÇáÚÇÆáÉ", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFamilyDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseDto<bool>.Fail(errors, "İÔá ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ", 400));
            }

            var result = await _familyService.UpdateAsync(id, dto, GetCurrentUserId());

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "Êã ÊÍÏíË ÇáÚÇÆáÉ ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "İÔá İí ÊÍÏíË ÇáÚÇÆáÉ", 400));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _familyService.DeleteAsync(id);

            return result.IsSuccess
                ? Ok(ApiResponseDto<bool>.Success(result.Data, "Êã ÍĞİ ÇáÚÇÆáÉ ÈäÌÇÍ"))
                : BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "İÔá İí ÍĞİ ÇáÚÇÆáÉ", 400));
        }

        /// <summary>
        /// Get family details with related teachers and students
        /// Accepts single or multiple family IDs (required at least one)
        /// </summary>
        [HttpGet("details")]
        public async Task<IActionResult> GetFamilyDetails(
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
                var result = await _familyService.GetFamilyDetailsAsync(singleId);
                return result.IsSuccess
                    ? Ok(ApiResponseDto<FamilyDetailsDto>.Success(result.Data!, "Êã ÇÓÊÑÌÇÚ ÊİÇÕíá ÇáÚÇÆáÉ ÈäÌÇÍ"))
                    : NotFound(ApiResponseDto<FamilyDetailsDto>.Fail(result.Errors, "áã íÊã ÇáÚËæÑ ÚáíåÇ", 404));
            }

            // áæ ÈÚÊ ÚÏÉ IDs
            var multipleResult = await _familyService.GetMultipleFamiliesDetailsAsync(distinctIds);
            return multipleResult.IsSuccess
                ? Ok(ApiResponseDto<List<FamilyDetailsDto>>.Success(multipleResult.Data!, "Êã ÇÓÊÑÌÇÚ ÊİÇÕíá ÇáÚÇÆáÇÊ ÈäÌÇÍ"))
                : NotFound(ApiResponseDto<List<FamilyDetailsDto>>.Fail(multipleResult.Errors, "İÔá İí ÌáÈ ÇáÈíÇäÇÊ", 404));
        }
    }
}
