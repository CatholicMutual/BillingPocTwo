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

            modelBuilder.Entity<UserRole>().ToTable("UserRoles"); // Map to UserRoles table
            modelBuilder.Entity<UserRole>().HasKey(ur => ur.Id); // Define primary key
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Roles) // Updated navigation property
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserUserRole>()
                .HasKey(uur => new { uur.UserId, uur.RoleId }); // Composite primary key

            modelBuilder.Entity<UserUserRole>()
                .HasOne(uur => uur.User)
                .WithMany(u => u.UserUserRoles) // Updated navigation property
                .HasForeignKey(uur => uur.UserId);

            modelBuilder.Entity<UserUserRole>()
                .HasOne(uur => uur.Role)
                .WithMany()
                .HasForeignKey(uur => uur.RoleId);
        }
    }
}
