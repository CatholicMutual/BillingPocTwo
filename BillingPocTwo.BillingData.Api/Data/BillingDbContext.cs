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
        public DbSet<ENTITY_ADDRESS_INFO> EntityAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ENTITY_REGISTER table mapping if needed
            modelBuilder.Entity<ENTITY_REGISTER>()
                .ToTable("ENTITY_REGISTER")
                .HasKey(e => e.SYSTEM_ENTITY_CODE);

            modelBuilder.Entity<ENTITY_ADDRESS_INFO>()
                .ToTable("ENTITY_ADDRESS_INFO")
                .HasKey(e => e.SEQ_ENTITY_ADDRESS_INFO);
        }

        void IBillingDbContext.OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
        }
    }
}
