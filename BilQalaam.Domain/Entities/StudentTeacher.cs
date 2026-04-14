namespace BilQalaam.Domain.Entities
{
    /// <summary>
    /// ÌÏæá æÓíØ ááÚáÇŞÉ Many-to-Many Èíä ÇáØÇáÈ æÇáãÚáã
    /// </summary>
    public class StudentTeacher
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
