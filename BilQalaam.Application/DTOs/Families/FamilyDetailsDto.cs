using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.DTOs.Teachers;

namespace BilQalaam.Application.DTOs.Families
{
    public class FamilyDetailsDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        // ÇáãÚáãíä ÇáãÑÊÈØíä ÈÇáÚÇÆáÉ (ãä ÎáÇá Supervisor)
        public List<TeacherResponseDto> Teachers { get; set; } = new();

        // ÇáØáÇÈ ÇáãÑÊÈØíä ÈÇáÚÇÆáÉ
        public List<StudentResponseDto> Students { get; set; } = new();

        // ÇáÅÍÕÇÆíÇÊ
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
    }
}
