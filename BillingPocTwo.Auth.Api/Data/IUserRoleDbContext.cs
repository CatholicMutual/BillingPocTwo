using BillingPocTwo.Shared.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.Auth.Api.Data
{
    public interface IUserRoleDbContext
    {
        DbSet<ROLE_MASTER> RoleMasters { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
