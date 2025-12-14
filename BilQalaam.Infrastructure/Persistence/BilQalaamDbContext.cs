using BilQalaam.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Infrastructure.Persistence
{
    // 🔹 DbContext هو المسؤول عن إدارة الاتصال بقاعدة البيانات وتوليد الجداول من الـ Entities
    // 🔹 يرث من IdentityDbContext علشان يشمل كل جداول الـ Identity (Users, Roles, Claims...)
    public class BilQalaamDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public BilQalaamDbContext(DbContextOptions<BilQalaamDbContext> options)
            : base(options)
        {
        }

        // 🔹 هنا هتضيف كل DbSet (يعني كل جدول في قاعدة البيانات)
        // مثال: public DbSet<Lesson> Lessons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // هنا ممكن تضيف أي إعدادات خاصة بالجداول أو العلاقات
            // مثال:
            // builder.Entity<ApplicationUser>().Property(u => u.FullName).HasMaxLength(150);
        }

        // 🔹 منطق إضافي لتحديث CreatedAt و UpdatedAt تلقائيًا عند الحفظ
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ApplicationUser>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
