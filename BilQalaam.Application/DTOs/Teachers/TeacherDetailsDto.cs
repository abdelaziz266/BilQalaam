using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Students;

namespace BilQalaam.Application.DTOs.Teachers
{
    public class TeacherDetailsDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // ÇáÚÇÆáÇÊ ÇáãÑÊÈØÉ ÈÇáãÚáã (ãä ÎáÇá Supervisor)
        public List<FamilyResponseDto> Families { get; set; } = new();

        // ÇáØáÇÈ ÇáãÑÊÈØíä ÈÇáãÚáã
        public List<StudentResponseDto> Students { get; set; } = new();

        // ÇáÅÍÕÇÆíÇÊ
        public int TotalFamilies { get; set; }
        public int TotalStudents { get; set; }
    }
}
