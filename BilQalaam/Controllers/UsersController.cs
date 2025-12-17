using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Users;
using BilQalaam.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BilQalaam.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ✅ GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<UserResponseDto>>.Success(users, "Users retrieved successfully"));
        }

        // ✅ GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user == null
                ? NotFound(ApiResponseDto<UserResponseDto>.Fail(new List<string> { "User not found" }, "Not found", 404))
                : Ok(ApiResponseDto<UserResponseDto>.Success(user, "User retrieved successfully"));
        }
    }
}
