using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Families
{
    public class FamilyResponseDto
    {
        public int Id { get; set; }
        
        // ?? »Ì«‰«  «·ÌÊ“—
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // ??û??û?? »Ì«‰«  «·⁄«∆·…
        public string FamilyName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        // ?? «·„‘—›
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }

        // ?? »Ì«‰«  „«·Ì…
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
