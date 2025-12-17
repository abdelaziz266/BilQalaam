using BilQalaam.Domain.Common;
using BilQalaam.Domain.Enums;

namespace BilQalaam.Domain.Entities
{
    public class Student : Base
    {
        // ?? ÈíÇäÇÊ ÇáØÇáÈ ÇáÃÓÇÓíÉ
        public string StudentName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ?? ÓÚÑ ÇáÓÇÚÉ
        public decimal HourlyRate { get; set; }
        public Currency Currency { get; set; }

        // ?????? ÇáÚÇÆáÉ ÇáÊÇÈÚ áåÇ (ßá ØÇáÈ áå ÚÇÆáÉ æÇÍÏÉ)
        public int FamilyId { get; set; }
        public Family Family { get; set; } = null!;

        // ???? ÇáãÚáã ÇáÊÇÈÚ áå (ßá ØÇáÈ áå ãÚáã æÇÍÏ)
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        // ?? ÇáÇÑÊÈÇØ ãÚ User (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
