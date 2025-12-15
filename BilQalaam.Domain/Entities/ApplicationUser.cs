using BilQalaam.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BilQalaam.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid BaseId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // 🧍‍♂️ بيانات المستخدم الشخصية
        public string FullName { get; set; } = string.Empty;
        public string? FamilyName { get; set; }
        public Gender? Gender { get; set; }   // ⬅ Enum بدل string
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }

        // 💼 بيانات إدارية
        public UserRole? Role { get; set; }   // ⬅ Enum بدل string
        public string? ContractType { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? ContractDate { get; set; }

        // ⭐ تقييم المستخدم
        public double? Rate { get; set; }

        public override string PhoneNumber { get; set; }
        public override string Email { get; set; }
    }
}
