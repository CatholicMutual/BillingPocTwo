using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Auth.Api.Test
{
    public class TestUserDbContext : UserDbContext
    {
        public TestUserDbContext(DbContextOptions<UserDbContext> options, DbSet<User> users) : base(options)
        {
            Users = users;
        }

        public override DbSet<User> Users { get; }

    }
}
