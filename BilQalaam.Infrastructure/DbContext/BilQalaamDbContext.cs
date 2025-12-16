using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Infrastructure.Persistence
{
    public class BilQalaamDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public BilQalaamDbContext(DbContextOptions<BilQalaamDbContext> options)
            : base(options)
        {
        }

        // ✅ DbSets فقط
        public DbSet<Lesson> Lessons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Lesson>()
                .HasOne(l => l.Family)
                .WithMany()
                .HasForeignKey(l => l.FamilyId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ مهم

            builder.Entity<Lesson>()
                .HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ مهم
        }
    }
}
