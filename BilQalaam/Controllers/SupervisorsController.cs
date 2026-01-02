using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BilQalaam.Api.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorsController : ControllerBase
    {
        private readonly ISupervisorService _supervisorService;

        public SupervisorsController(ISupervisorService supervisorService)
        {
            _supervisorService = supervisorService;
        }

        // ?? Helper method ··Õ’Ê· ⁄·Ï User ID «·Õ«·Ì
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        // ? GET: api/Supervisors/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _supervisorService.GetAllAsync(pageNumber, pageSize, searchText);

            return result.IsSuccess
                ? Ok(ApiResponseDto<PaginatedResponseDto<SupervisorResponseDto>>.Success(
                    result.Data!,
                    " „ «” —Ã«⁄ «·„‘—›Ì‰ »‰Ã«Õ"
                ))
                : BadRequest(ApiResponseDto<PaginatedResponseDto<SupervisorResponseDto>>.Fail(
                    result.Errors,
                    "›‘· ›Ì Ã·» «·„‘—›Ì‰",
                    400
                ));
        }

        // ? GET: api/Supervisors/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supervisor = await _supervisorService.GetByIdAsync(id);
            return supervisor == null
                ? NotFound(ApiResponseDto<SupervisorResponseDto>.Fail(
                    new List<string> { "«·„‘—› €Ì— „ÊÃÊœ" },
                    "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ",
                    404
                ))
                : Ok(ApiResponseDto<SupervisorResponseDto>.Success(
                    supervisor,
                    " „ «” —Ã«⁄ «·„‘—› »‰Ã«Õ"
                ));
        }

        // ? POST: api/Supervisors/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSupervisorDto dto)
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
            var result = await _supervisorService.CreateAsync(dto, currentUserId);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<int>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<int>.Success(result.Data, " „ ≈‰‘«¡ «·„‘—› »‰Ã«Õ", 201));
        }

        // ? PUT: api/Supervisors/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupervisorDto dto)
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
            var result = await _supervisorService.UpdateAsync(id, dto, currentUserId);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<bool>.Success(result.Data, " „  ÕœÌÀ «·„‘—› »‰Ã«Õ"));
        }

        // ? DELETE: api/Supervisors/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supervisorService.DeleteAsync(id);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseDto<bool>.Fail(result.Errors, "›‘· «· Õﬁﬁ „‰ «·»Ì«‰« ", 400));

            return Ok(ApiResponseDto<bool>.Success(result.Data, " „ Õ–› «·„‘—› »‰Ã«Õ"));
        }
        [HttpGet("details")]
        public async Task<IActionResult> GetSupervisorDetails(
            [FromQuery(Name = "ids")] IEnumerable<int>? ids = null)
        {
            // «· Õﬁﬁ „‰ √‰ Â‰«ﬂ „⁄—› Ê«Õœ ⁄·Ï «·√ﬁ·
            if (ids == null || !ids.Any())
            {
                return BadRequest(ApiResponseDto<object>.Fail(
                    new List<string> { "ÌÃ»  ÕœÌœ „⁄—› Ê«Õœ ⁄·Ï «·√ﬁ·" },
                    "»Ì«‰«  „›ﬁÊœ…",
                    400
                ));
            }

            var distinctIds = ids.Distinct().ToList();

            // ·Ê »⁄  single ID
            if (distinctIds.Count == 1)
            {
                var singleId = distinctIds.First();
                var result = await _supervisorService.GetSupervisorDetailsAsync(singleId);
                return result.IsSuccess
                    ? Ok(ApiResponseDto<SupervisorDetailsDto>.Success(result.Data!, " „ «” —Ã«⁄  ›«’Ì· «·„‘—› »‰Ã«Õ"))
                    : NotFound(ApiResponseDto<SupervisorDetailsDto>.Fail(result.Errors, "·„ Ì „ «·⁄ÀÊ— ⁄·ÌÂ", 404));
            }

            // ·Ê »⁄  ⁄œ… IDs
            var multipleResult = await _supervisorService.GetMultipleSupervisorsDetailsAsync(distinctIds);
            return multipleResult.IsSuccess
                ? Ok(ApiResponseDto<List<SupervisorDetailsDto>>.Success(multipleResult.Data!, " „ «” —Ã«⁄  ›«’Ì· «·„‘—›Ì‰ »‰Ã«Õ"))
                : NotFound(ApiResponseDto<List<SupervisorDetailsDto>>.Fail(multipleResult.Errors, "›‘· ›Ì Ã·» «·»Ì«‰« ", 404));
        }
    }
}
