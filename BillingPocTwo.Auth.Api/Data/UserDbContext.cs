using Microsoft.EntityFrameworkCore;
using BillingPocTwo.Shared.Entities.Auth;

namespace BillingPocTwo.Auth.Api.Data
{
    public class UserDbContext : DbContext, IUserDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserUserRole",
                    j => j.HasOne<UserRole>().WithMany().HasForeignKey("UserRoleId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"));
        }
    }
}
