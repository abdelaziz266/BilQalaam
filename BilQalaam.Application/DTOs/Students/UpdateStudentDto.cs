using BilQalaam.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Students
{
    public class UpdateStudentDto
    {
        // ?? ÈíÇäÇÊ ÇáãÓÊÎÏã
        [Required(ErrorMessage = "ÇáÇÓã ÇáßÇãá ãØáæÈ")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ÇáÇÓã íÌÈ Ãä íßæä Èíä 3 æ 100 ÍÑİ")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ÑŞã ÇáåÇÊİ ãØáæÈ")]
        [RegularExpression(@"^(010|011|012)\d{8}$", ErrorMessage = "ÑŞã ÇáåÇÊİ íÌÈ Ãä íÈÏÃ È 010 Ãæ 011 Ãæ 012 æíßæä 11 ÑŞã")]
        public string PhoneNumber { get; set; } = string.Empty;


        // ?? ÃÌÑ ÇáØÇáÈ
        [Range(0.01, double.MaxValue, ErrorMessage = "ÓÚÑ ÇáÓÇÚÉ íÌÈ Ãä íßæä ÃßÈÑ ãä 0")]
        public decimal HourlyRate { get; set; }

        public Currency Currency { get; set; }

        // ?? ÇáÚÇÆáÉ ÇáãæßæáÉ ááØÇáÈ (ID ãä ÌÏæá Families)
        [Range(1, int.MaxValue, ErrorMessage = "íÌÈ ÇÎÊíÇÑ ÚÇÆáÉ ÕÍíÍÉ")]
        public int FamilyId { get; set; }

        // ?? ÇáãÚáã ÇáãÓÄæá Úäå (ID ãä ÌÏæá Teachers)
        [Range(1, int.MaxValue, ErrorMessage = "íÌÈ ÇÎÊíÇÑ ãÚáã ÕÍíÍ")]
        public int TeacherId { get; set; }

        // ?? ßáãÉ ÇáãÑæÑ (ÇÎÊíÇÑí)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "ßáãÉ ÇáãÑæÑ íÌÈ Ãä Êßæä Èíä 6 æ 100 ÍÑİ")]
        public string? Password { get; set; }
    }
}
