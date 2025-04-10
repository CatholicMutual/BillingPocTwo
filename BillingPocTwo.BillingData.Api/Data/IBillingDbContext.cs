using BillingPocTwo.Shared.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BillingPocTwo.BillingData.Api.Data
{
    public interface IBillingDbContext
    {
        DbSet<ENTITY_REGISTER> EntityRegisters { get; set; }
        DbSet<ENTITY_ADDRESS_INFO> EntityAddresses { get; set; }
        DbSet<POLICY_ENTITY_REGISTER> PolicyEntityIntermediate { get; set; }
        DbSet<POLICY_REGISTER> PolicyRegisters { get; set; }

        void OnModelCreating(ModelBuilder modelBuilder);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry Entry(object entity);

    }
}
