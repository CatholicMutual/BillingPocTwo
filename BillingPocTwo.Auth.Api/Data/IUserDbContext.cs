using System.Threading;
using System.Threading.Tasks;
using BillingPocTwo.Shared.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.Auth.Api.Data
{
    public interface IUserDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<ROLE_MASTER> UserRoles { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
