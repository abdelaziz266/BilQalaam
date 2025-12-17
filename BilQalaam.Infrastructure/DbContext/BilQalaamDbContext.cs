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

        // ✅ DbSets
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal precision
            builder.Entity<ApplicationUser>()
                .Property(u => u.Salary)
                .HasPrecision(18, 2);

            builder.Entity<Family>()
                .Property(f => f.HourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<Teacher>()
                .Property(t => t.HourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<Supervisor>()
                .Property(s => s.HourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<Student>()
                .Property(s => s.HourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<Lesson>()
                .Property(l => l.StudentHourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<Lesson>()
                .Property(l => l.TeacherHourlyRate)
                .HasPrecision(18, 2);

            // Lesson → Student relationship
            builder.Entity<Lesson>()
                .HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ مهم

            // Lesson → Teacher relationship
            builder.Entity<Lesson>()
                .HasOne(l => l.Teacher)
                .WithMany()
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ مهم

            // Lesson → Family relationship
            builder.Entity<Lesson>()
                .HasOne(l => l.Family)
                .WithMany()
                .HasForeignKey(l => l.FamilyId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ مهم

            // Family → User relationship (One-to-One)
            builder.Entity<Family>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Family → Supervisor relationship
            builder.Entity<Family>()
                .HasOne(f => f.Supervisor)
                .WithMany()
                .HasForeignKey(f => f.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Teacher → User relationship (One-to-One)
            builder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Teacher → Supervisor relationship
            builder.Entity<Teacher>()
                .HasOne(t => t.Supervisor)
                .WithMany()
                .HasForeignKey(t => t.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Supervisor → User relationship (One-to-One)
            builder.Entity<Supervisor>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student → User relationship (One-to-One)
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student → Family relationship (Many-to-One)
            builder.Entity<Student>()
                .HasOne(s => s.Family)
                .WithMany()
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student → Teacher relationship (Many-to-One)
            builder.Entity<Student>()
                .HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
