using BilQalaam.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Students
{
    public class UpdateStudentDto
    {
        // ?? »Ì«‰«  «·ÿ«·»
        [Required(ErrorMessage = "«”„ «·ÿ«·» „ÿ·Ê»")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "«”„ ÌÃ» √‰ ÌﬂÊ‰ »Ì‰ 3 Ê 100 Õ—›")]
        public string FullName { get; set; } = string.Empty;

        // ?? —„“ «·œÊ·… + —ﬁ„ «·Â« ›
        [Required(ErrorMessage = "—„“ «·œÊ·… „ÿ·Ê»")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "—„“ «·œÊ·… ÌÃ» √‰ ÌﬂÊ‰ »Ì‰ 1 Ê 5 √Õ—›")]
        public string CountryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "—ﬁ„ «·Â« › „ÿ·Ê»")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "—ﬁ„ «·Â« › ÌÃ» √‰ ÌﬂÊ‰ »Ì‰ 10 Ê 15 —ﬁ„")]
        public string PhoneNumber { get; set; } = string.Empty;

        // ?? ”⁄— «·”«⁄…
        [Range(0.01, double.MaxValue, ErrorMessage = "”⁄— «·”«⁄… ÌÃ» √‰ ÌﬂÊ‰ √ﬂ»— „‰ 0")]
        public decimal HourlyRate { get; set; }

        public Currency Currency { get; set; }

        // ?? «·⁄·«ﬁ«  «·Œ«—ÃÌ…
        public int? FamilyId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "«”„ «·„⁄·„ „ÿ·Ê»")]
        public int TeacherId { get; set; }

        // ?? ﬂ·„… «·„—Ê— («Œ Ì«—Ì…)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "ﬂ·„… «·„—Ê— ÌÃ» √‰  ﬂÊ‰ »Ì‰ 6 Ê 100 Õ—›")]
        public string? Password { get; set; }
    }
}
