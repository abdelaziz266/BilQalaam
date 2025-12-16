using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilQalaam.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _lessonService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lesson = await _lessonService.GetByIdAsync(id);
            return lesson == null ? NotFound() : Ok(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            var id = await _lessonService.CreateAsync(dto);
            return Ok(new { message = "Lesson created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonDto dto)
        {
            var success = await _lessonService.UpdateAsync(id, dto);
            return success ? Ok("Lesson updated successfully") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _lessonService.DeleteAsync(id);
            return success ? Ok("Lesson deleted successfully") : NotFound();
        }
    }
}
