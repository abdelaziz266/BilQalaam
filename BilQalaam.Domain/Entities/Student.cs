using BilQalaam.Domain.Common;

namespace BilQalaam.Domain.Entities
{
    public class Student : Base
    {
        // ???? ÇÓã ÇáØÇáÈ
        public string StudentName { get; set; } = string.Empty;

        // ?????? ÇáÚáÇŞÉ ãÚ ÇáÚÇÆáÉ (Many-to-One)
        public int? FamilyId { get; set; }
        public Family Family { get; set; } = null!;

        // ???? ÇáÚáÇŞÉ ãÚ ÇáãÚáã (Many-to-One)
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;
    }
}
