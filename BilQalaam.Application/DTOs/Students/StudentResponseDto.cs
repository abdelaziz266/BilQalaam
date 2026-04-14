namespace BilQalaam.Application.DTOs.Students
{
    public class StudentResponseDto
    {
        public int Id { get; set; }

        // ?? ÇáØÇáÈ
        public string StudentName { get; set; } = string.Empty;

        // ?????? ÇáÚÇÆáÉ
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        // ???? ÇáãÚáãíä
        public List<TeacherInfoDto> Teachers { get; set; } = new();
    }

    public class TeacherInfoDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
    }
}
