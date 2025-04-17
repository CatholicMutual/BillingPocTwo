using Microsoft.EntityFrameworkCore;
using BillingPocTwo.Shared.Entities.Auth;

namespace BillingPocTwo.Auth.Api.Data
{
    public class UserRoleDbContext : DbContext, IUserRoleDbContext
    {
        public UserRoleDbContext(DbContextOptions<UserRoleDbContext> options) : base(options)
        {
        }

        public DbSet<ROLE_MASTER> RoleMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ROLE_MASTER>().ToTable("ROLE_MASTER"); // Ensure the table name matches the database
            modelBuilder.Entity<ROLE_MASTER>().HasKey(r => r.SEQ_ROLE_MASTER);
        }
    }
}
