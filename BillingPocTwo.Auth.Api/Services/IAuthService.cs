using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Auth.Api.Models;

namespace BillingPocTwo.Auth.Api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task LogoutAsync(Guid id);
        Task<bool> DeleterUserAsync(string email);
        Task<bool> ChangeUserRoleAsync(string email, string newRole);

    }
}
