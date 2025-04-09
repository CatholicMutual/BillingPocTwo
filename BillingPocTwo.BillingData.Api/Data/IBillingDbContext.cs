using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.BillingData.Api.Data
{
    public interface IBillingDbContext
    {
        DbSet<ENTITY_REGISTER> EntityRegisters { get; set; }

        void OnModelCreating(ModelBuilder modelBuilder);
    }
}
