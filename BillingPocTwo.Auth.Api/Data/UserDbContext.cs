using Microsoft.EntityFrameworkCore;
using BillingPocTwo.Auth.Api.Controllers;

namespace BillingPocTwo.Auth.Api.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
    }
}
