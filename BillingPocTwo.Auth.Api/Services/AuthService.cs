using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Auth.Api.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        
        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if ((user is null) || (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed))
            {
                return null;
            }

            TokenResponseDto response = await CreateTokenResponse(user);

            return response;
        }

        public async Task LogoutAsync(Guid userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiration = null;
                await context.SaveChangesAsync();
            }
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Email.ToUpper() == request.Email.ToUpper()))
            {
                return null;
            }

            var user = new User();

            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Email = request.Email;
            user.PasswordHash = hashedPassword;
            user.Role = await context.UserRoles.FirstOrDefaultAsync(r => r.Name == "User");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> ChangeUserRoleAsync(string email, string newRole)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            var role = await context.UserRoles.FirstOrDefaultAsync(r => r.Name == newRole);
            if (role == null)
            {
                return false;
            }

            user.UserRoleId = role.Id;
            user.Role = role;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleterUserAsync(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = await CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private async Task<string> CreateToken(User user)
        {
            var userRole = await GetRoleNameByIdAsync(user.UserRoleId ?? 2);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, userRole ?? "User") // Add role claim
            };

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSettings:Secret")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                    issuer: configuration.GetValue<string>("JwtSettings:Issuer"),
                    audience: configuration.GetValue<string>("JwtSettings:Audience"),
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        public async Task<int?> GetRoleIdByNameAsync(string roleName)
        {
            var role = await context.UserRoles.FirstOrDefaultAsync(r => r.Name == roleName);
            return role?.Id;
        }

        public async Task<string?> GetRoleNameByIdAsync(int userRoleId)
        {
            var role = await context.UserRoles.FirstOrDefaultAsync(r => r.Id == userRoleId);
            return role?.Name;
        }

    }
}
