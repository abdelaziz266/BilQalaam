using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Families;
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
    public class FamiliesController : ControllerBase
    {
        private readonly IFamilyService _familyService;

        public FamiliesController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        // ?? Helper method ··Õ’Ê· ⁄·Ï User ID «·Õ«·Ì
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        // ? GET: api/Families
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var families = await _familyService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<FamilyResponseDto>>.Success(families, "Families retrieved successfully"));
        }

        // ? GET: api/Families/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            return family == null 
                ? NotFound(ApiResponseDto<FamilyResponseDto>.Fail(new List<string> { "Family not found" }, "Not found", 404))
                : Ok(ApiResponseDto<FamilyResponseDto>.Success(family, "Family retrieved successfully"));
        }

        // ? POST: api/Families
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
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
                var id = await _familyService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, "Family created successfully", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "Validation failed", 400));
            }
        }

        // ? PUT: api/Families/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFamilyDto dto)
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
            var success = await _familyService.UpdateAsync(id, dto, currentUserId);
            return success 
                ? Ok(ApiResponseDto<bool>.Success(true, "Family updated successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Family not found" }, "Not found", 404));
        }

        // ? DELETE: api/Families/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _familyService.DeleteAsync(id);
            return success 
                ? Ok(ApiResponseDto<bool>.Success(true, "Family deleted successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Family not found" }, "Not found", 404));
        }
    }
}
