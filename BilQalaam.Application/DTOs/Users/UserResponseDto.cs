using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Users
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? FamilyName { get; set; }
        public Gender? Gender { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
