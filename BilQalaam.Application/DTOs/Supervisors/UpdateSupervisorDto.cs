using BilQalaam.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Supervisors
{
    public class UpdateSupervisorDto
    {
        // ?? »Ì«‰«  «·„” Œœ„
        [Required(ErrorMessage = "«·«”„ «·ﬂ«„· „ÿ·Ê»")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "«·«”„ ÌÃ» √‰ ÌﬂÊ‰ »Ì‰ 3 Ê 100 Õ—›")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "—ﬁ„ «·Â« › „ÿ·Ê»")]
        [RegularExpression(@"^(010|011|012)\d{8}$", ErrorMessage = "—ﬁ„ «·Â« › ÌÃ» √‰ Ì»œ√ » 010 √Ê 011 √Ê 012 ÊÌﬂÊ‰ 11 —ﬁ„")]
        public string PhoneNumber { get; set; } = string.Empty;

        

        // ?? √Ã— «·„‘—›
        [Range(0.01, double.MaxValue, ErrorMessage = "”⁄— «·”«⁄… ÌÃ» √‰ ÌﬂÊ‰ √ﬂ»— „‰ 0")]
        public decimal HourlyRate { get; set; }

        public Currency Currency { get; set; }

        // ?? ﬂ·„… «·„—Ê— («Œ Ì«—Ì)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "ﬂ·„… «·„—Ê— ÌÃ» √‰  ﬂÊ‰ »Ì‰ 6 Ê 100 Õ—›")]
        public string? Password { get; set; }
    }
}
