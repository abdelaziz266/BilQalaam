using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Supervisors;
using BilQalaam.Application.Exceptions;
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

        // ? GET: api/Supervisors
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var supervisors = await _supervisorService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<SupervisorResponseDto>>.Success(supervisors, "Supervisors retrieved successfully"));
        }

        // ? GET: api/Supervisors/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supervisor = await _supervisorService.GetByIdAsync(id);
            return supervisor == null
                ? NotFound(ApiResponseDto<SupervisorResponseDto>.Fail(new List<string> { "Supervisor not found" }, "Not found", 404))
                : Ok(ApiResponseDto<SupervisorResponseDto>.Success(supervisor, "Supervisor retrieved successfully"));
        }

        // ? POST: api/Supervisors
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupervisorDto dto)
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
                var id = await _supervisorService.CreateAsync(dto, currentUserId);
                return Ok(ApiResponseDto<int>.Success(id, "Supervisor created successfully", 201));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseDto<int>.Fail(ex.Errors, "Validation failed", 400));
            }
        }

        // ? PUT: api/Supervisors/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupervisorDto dto)
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
            var success = await _supervisorService.UpdateAsync(id, dto, currentUserId);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Supervisor updated successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Supervisor not found" }, "Not found", 404));
        }

        // ? DELETE: api/Supervisors/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _supervisorService.DeleteAsync(id);
            return success
                ? Ok(ApiResponseDto<bool>.Success(true, "Supervisor deleted successfully"))
                : NotFound(ApiResponseDto<bool>.Fail(new List<string> { "Supervisor not found" }, "Not found", 404));
        }
    }
}
