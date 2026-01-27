using BilQalaam.Application.DTOs.Common;

namespace BilQalaam.Application.DTOs.Lessons
{
    public class LessonPaginatedResponseDto : PaginatedResponseDto<LessonResponseDto>
    {
        public double TotalHours { get; set; }
    }
}
