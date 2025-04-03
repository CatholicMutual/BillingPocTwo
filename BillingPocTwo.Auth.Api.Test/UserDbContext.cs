using BillingPocTwo.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Auth.Api.Test
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options, DbSet<User> users = null) : base(options)
        {
            Users = users ?? Set<User>();
        }

        public virtual DbSet<User> Users { get; }
    }
}
