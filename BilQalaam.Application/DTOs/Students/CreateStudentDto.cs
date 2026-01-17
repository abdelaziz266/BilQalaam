using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Students
{
    public class CreateStudentDto
    {
        // ???? ÇÓã ÇáØÇáÈ
        [Required(ErrorMessage = "ÇÓã ÇáØÇáÈ ãØáæÈ")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ÇÓã íÌÈ Ãä íßæä Èíä 3 æ 100 ÍÑİ")]
        public string FullName { get; set; } = string.Empty;

        // ?????? ÇáÚÇÆáÉ
        [Required(ErrorMessage = "ÇáÚÇÆáÉ ãØáæÈÉ")]
        [Range(1, int.MaxValue, ErrorMessage = "ãÚÑİ ÇáÚÇÆáÉ ãØáæÈ")]
        public int FamilyId { get; set; }

        // ???? ÇáãÚáã
        [Required(ErrorMessage = "ÇáãÚáã ãØáæÈ")]
        [Range(1, int.MaxValue, ErrorMessage = "ãÚÑİ ÇáãÚáã ãØáæÈ")]
        public int TeacherId { get; set; }
    }
}
