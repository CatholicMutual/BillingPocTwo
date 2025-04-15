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
        public DbSet<POLICY_ENTITY_REGISTER> PolicyEntityIntermediate { get; set; }
        public DbSet<POLICY_REGISTER> PolicyRegisters { get; set; }
        public DbSet<TRANSACTION_LOG> TransactionLogs { get; set; }

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

            modelBuilder.Entity<POLICY_ENTITY_REGISTER>()
                .ToTable("POLICY_ENTITY_REGISTER")
                .HasKey(e => new { e.POLICY_TERM_ID, e.ENTITY_TYPE, e.SYSTEM_ENTITY_CODE, e.SYSTEM_ACTIVITY_NO, e.SYSTEM_TRANSACTION_SEQ });

            modelBuilder.Entity<POLICY_REGISTER>()
                .ToTable("POLICY_REGISTER")
                .HasKey(e => e.POLICY_TERM_ID);

            modelBuilder.Entity<TRANSACTION_LOG>()
                .ToTable("TRANSACTION_LOG")
                .HasKey(e => e.SYSTEM_TRANSACTION_SEQ);
        }

        void IBillingDbContext.OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder);
        }
    }
}
