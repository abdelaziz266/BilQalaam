using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Families
{
    public class CreateFamilyDto
    {
        // ?? »Ì«‰«  «·ÌÊ“—
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

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
