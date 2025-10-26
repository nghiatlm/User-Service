using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.BO.Entities;

namespace UserService.Repository
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.RoleName)
                      .HasConversion<string>()
                      .HasColumnName("role_name")
                      .HasColumnType("nvarchar(50)");

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasColumnName("status")
                      .HasColumnType("nvarchar(50)");

                entity.Property(e => e.UserName).HasColumnName("user_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.Avatar).HasColumnName("avatar");

                // Use MySQL-friendly types and default for created_at
                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at")
                      .HasColumnType("datetime(6)")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Keep UpdatedAt managed by the application (UpdateTimestamps),
                // use MySQL datetime precision
                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasColumnType("datetime(6)");
            });
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<User>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}