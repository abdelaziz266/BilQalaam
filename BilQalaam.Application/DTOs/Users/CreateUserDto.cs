using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.DTOs.Users
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // هتتحول من string إلى Enum تلقائيًا عن طريق الـ Mapper
        public UserRole? Role { get; set; }
        public Gender? Gender { get; set; }

        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ContractType { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? ContractDate { get; set; }
    }
}
