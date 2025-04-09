using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities.Auth;

namespace BillingPocTwo.Auth.Api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(CreateUserDto request, bool changePasswordOnFirstLogin);
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task LogoutAsync(Guid id);
        Task<bool> DeleteUserAsync(string currentUserEmail, string email);
        Task<bool> ChangeUserRoleAsync(string currentUserEmail, string email, List<string> newRoles);
        Task<bool> UpdateUserAsync(User user, string modifiedBy);

    }
}
