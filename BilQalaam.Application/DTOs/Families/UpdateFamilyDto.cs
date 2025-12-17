using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Families
{
    public class UpdateFamilyDto
    {
        // ?? »Ì«‰«  «·ÌÊ“—
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // ??û??û?? »Ì«‰«  «·⁄«∆·…
        public string FamilyName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        // ?? «·„‘—› «· «»⁄ ·Â
        public int? SupervisorId { get; set; }

        // ?? »Ì«‰«  „«·Ì…
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }
    }
}
