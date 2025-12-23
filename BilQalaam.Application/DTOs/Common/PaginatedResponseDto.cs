namespace BilQalaam.Application.DTOs.Common
{
    public class PaginatedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int PagesCount { get; set; }
    }
}
