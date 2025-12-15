using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FamilyName { get; set; }
        public Gender? Gender { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ContractType { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? ContractDate { get; set; }
    }
}
