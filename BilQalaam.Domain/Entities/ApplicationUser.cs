using Microsoft.AspNetCore.Identity;
using System;

namespace BilQalaam.Domain.Entities
{
    // يرث من IdentityUser لتفعيل هوية المستخدم (Login, Password, Roles...)
    public class ApplicationUser : IdentityUser
    {
        // 🧱 الخصائص العامة المشتركة (من BaseEntity)
        public Guid BaseId { get; set; } = Guid.NewGuid(); // معرّف خاص بالمستخدم داخل النظام
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // 🧍‍♂️ بيانات المستخدم الشخصية
        public string FullName { get; set; } = string.Empty; // الاسم الكامل
        public string? FamilyName { get; set; }               // العائلة
        public string? Gender { get; set; }                   // النوع (ذكر / أنثى)
        public int? Age { get; set; }                         // العمر
        public DateTime? BirthDate { get; set; }              // تاريخ الميلاد

        // 💼 بيانات إدارية
        public string? Role { get; set; }                     // الدور (SuperAdmin, Admin, Teacher, Student)
        public string? ContractType { get; set; }             // نوع التكلفة أو العقد
        public decimal? Salary { get; set; }                  // السعر أو الأجر
        public DateTime? ContractDate { get; set; }           // تاريخ التعاقد

        // ⭐ تقييم المستخدم
        public double? Rate { get; set; }                     // التقييم

        // 📞 معلومات الاتصال (موروثة من IdentityUser)
        public override string PhoneNumber { get; set; }
        public override string Email { get; set; }
    }
}
