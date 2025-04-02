using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities;

namespace BillingPocTwo.Auth.Api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<User?> RegisterAsync(CreateUserDto request, bool changePasswordOnFirstLogin);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task LogoutAsync(Guid id);
        Task<bool> DeleterUserAsync(string email);
        Task<bool> ChangeUserRoleAsync(string email, List<string> newRoles);

    }
}
