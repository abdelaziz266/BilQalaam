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

        // ? GET: api/Families/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (families, totalCount) = await _familyService.GetAllAsync(pageNumber, pageSize);
            var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedResponse = new PaginatedResponseDto<FamilyResponseDto>
            {
                Items = families,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                PagesCount = pagesCount
            };

            return Ok(ApiResponseDto<PaginatedResponseDto<FamilyResponseDto>>.Success(
                paginatedResponse,
                " „ «” —Ã«⁄ «·⁄«∆·«  »‰Ã«Õ"
            ));
        }

        // ? GET: api/Families/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            return family == null 
                ? NotFound(ApiResponseDto<FamilyResponseDto>.Fail(
                    new List<string> { "«·⁄«∆·… €Ì— „ÊÃÊœ…" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ«",
                    404
                ))
                : Ok(ApiResponseDto<FamilyResponseDto>.Success(
                    family,
                    " „ «” —Ã«⁄ «·⁄«∆·… »‰Ã«Õ"
                ));
        }

        // ? POST: api/Families/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponseDto<int>.Fail(modelErrors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));
            }

            var currentUserId = GetCurrentUserId();
            var result = await _familyService.CreateAsync(dto, currentUserId);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<int>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<int>.Success(result.Data, " „ ≈‰‘«¡ «·⁄«∆·… »‰Ã«Õ", 201));
        }

        // ? PUT: api/Families/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFamilyDto dto)
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
            var result = await _familyService.UpdateAsync(id, dto, currentUserId);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<bool>.Success(result.Data, " „  ÕœÌÀ «·⁄«∆·… »‰Ã«Õ"));
        }

        // ? DELETE: api/Families/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _familyService.DeleteAsync(id);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<bool>.Success(result.Data, " „ Õ–› «·⁄«∆·… »‰Ã«Õ"));
        }
    }
}
