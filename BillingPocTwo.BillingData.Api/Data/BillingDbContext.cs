using Microsoft.EntityFrameworkCore;
using BillingPocTwo.Shared.Entities.Billing;
using System;

namespace BillingPocTwo.BillingData.Api.Data
{
    public class BillingDbContext : DbContext, IBillingDbContext
    {
        public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

        // Add DbSet for ENTITY_REGISTER
        public DbSet<ENTITY_REGISTER> EntityRegisters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ENTITY_REGISTER table mapping if needed
            modelBuilder.Entity<ENTITY_REGISTER>()
                .ToTable("ENTITY_REGISTER")
                .HasKey(e => e.SYSTEM_ENTITY_CODE);
        }

        void IBillingDbContext.OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
        }
    }
}
