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

        // ✅ GET: api/Users/get
        [HttpGet("get")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var (users, totalCount) = await _userService.GetAllAsync(pageNumber, pageSize);
            var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedResponse = new PaginatedResponseDto<UserResponseDto>
            {
                Items = users,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                PagesCount = pagesCount
            };

            return Ok(ApiResponseDto<PaginatedResponseDto<UserResponseDto>>.Success(
                paginatedResponse,
                "تم استرجاع المستخدمين بنجاح"
            ));
        }

        // ✅ GET: api/Users/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user == null
                ? NotFound(ApiResponseDto<UserResponseDto>.Fail(
                    new List<string> { "المستخدم غير موجود" },
                    "لم يتم العثور عليه",
                    404
                ))
                : Ok(ApiResponseDto<UserResponseDto>.Success(
                    user,
                    "تم استرجاع المستخدم بنجاح"
                ));
        }
    }
}
