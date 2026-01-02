using BilQalaam.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Teachers
{
    public class CreateTeacherDto
    {
        // ?? ÈíÇäÇÊ ÇáãÚáã ÇáÃÓÇÓíÉ
        [Required(ErrorMessage = "ÇÓã ÇáãÚáã ãØáæÈ")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ÇÓã íÌÈ Ãä íßæä Èíä 3 æ 100 ÍÑİ")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ÇáÈÑíÏ ÇáÅáßÊÑæäí ãØáæÈ")]
        [EmailAddress(ErrorMessage = "ÇáÈÑíÏ ÇáÅáßÊÑæäí ÛíÑ ÕÍíÍ")]
        public string Email { get; set; } = string.Empty;

        // ?? ÑãÒ ÇáÏæáÉ + ÑŞã ÇáåÇÊİ
        [Required(ErrorMessage = "ÑãÒ ÇáÏæáÉ ãØáæÈ")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "ÑãÒ ÇáÏæáÉ íÌÈ Ãä íßæä Èíä 1 æ 5 ÃÍÑİ")]
        public string CountryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "ÑŞã ÇáåÇÊİ ãØáæÈ")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "ÑŞã ÇáåÇÊİ íÌÈ Ãä íßæä Èíä 10 æ 15 ÑŞã")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "ßáãÉ ÇáãÑæÑ ãØáæÈÉ")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "ßáãÉ ÇáãÑæÑ íÌÈ Ãä Êßæä Èíä 6 æ 100 ÍÑİ")]
        public string Password { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        [Range(0.01, double.MaxValue, ErrorMessage = "ÓÚÑ ÇáÓÇÚÉ íÌÈ Ãä íßæä ÃßÈÑ ãä 0")]
        public decimal HourlyRate { get; set; }

        public Currency Currency { get; set; }

        // ?? ÊÇÑíÎ ÈÏÁ ÇáÚãá
        [Required(ErrorMessage = "ÊÇÑíÎ ÈÏÁ ÇáÚãá ãØáæÈ")]
        public DateTime StartDate { get; set; }

        // ?? ÇáãÔÑİ ÇáãÓÄæá Úä ÇáãÚáã (ID ãä ÌÏæá Supervisors)
        public int? SupervisorId { get; set; }
    }
}
