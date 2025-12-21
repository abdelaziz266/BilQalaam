using BilQalaam.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Users
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^(010|011|012)\d{8}$", ErrorMessage = "رقم الهاتف يجب أن يبدأ ب 010 أو 011 أو 012 ويكون 11 رقم")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون بين 6 و 100 حرف")]
        public string Password { get; set; } = string.Empty;

        // هتتحول من string إلى Enum تلقائيًا عن طريق الـ Mapper
        public UserRole? Role { get; set; }
        public Gender? Gender { get; set; }

        [Range(1, 120, ErrorMessage = "العمر يجب أن يكون بين 1 و 120")]
        public int? Age { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50, ErrorMessage = "نوع العقد يجب أن لا يتجاوز 50 حرف")]
        public string? ContractType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "الراتب يجب أن يكون أكبر من 0")]
        public decimal? Salary { get; set; }

        public DateTime? ContractDate { get; set; }
    }
}
