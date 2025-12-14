namespace BilQalaam.Domain.Common
{
    public class Base
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🕒 تواريخ الإنشاء والتحديث
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // 👤 المستخدمين المسؤولين عن الإنشاء والتحديث
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }

        // 🗑️ الحذف المنطقي (Soft Delete)
        public bool IsDeleted { get; set; } = false;
    }
}
