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
        public virtual DbSet<ROLE_MASTER> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the many-to-many relationship between User and ROLE_MASTER
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserUserRole",
                    j => j.HasOne<ROLE_MASTER>()
                          .WithMany()
                          .HasForeignKey("SEQ_ROLE_MASTER") // Use SEQ_ROLE_MASTER as FK
                          .HasConstraintName("FK_UserUserRole_RoleMaster")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<User>()
                          .WithMany()
                          .HasForeignKey("UserId")
                          .HasConstraintName("FK_UserUserRole_User")
                          .OnDelete(DeleteBehavior.Cascade));

            // Additional configuration for the join table
            modelBuilder.SharedTypeEntity<Dictionary<string, object>>("UserUserRole", entity =>
            {
                entity.ToTable("UserUserRole");
                entity.Property<int>("SEQ_ROLE_MASTER").IsRequired();
                entity.Property<Guid>("UserId").IsRequired();
            });
        }
    }
}
