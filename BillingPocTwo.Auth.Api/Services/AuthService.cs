using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BillingPocTwo.Auth.Api.Services
{
    public class AuthService(UserDbContext context, UserRoleDbContext rolesContext, IConfiguration configuration) : IAuthService
    {
        private readonly UserDbContext context = context;
        private readonly UserRoleDbContext rolesContext = rolesContext;
        private readonly IConfiguration configuration = configuration;

        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            var user = await context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user != null)
            {
                var validRoles = user.Roles.Where(r => !string.IsNullOrEmpty(r.RoleName)).ToList();
                user.Roles = validRoles;
            }

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

        public async Task<User?> RegisterAsync(CreateUserDto request, bool changePasswordOnFirstLogin = true)
        {
            if (await context.Users.AnyAsync(u => u.Email.ToUpper() == request.Email.ToUpper()))
            {
                return null;
            }

            var user = new User()
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = new PasswordHasher<User>().HashPassword(null, request.Password),
                ChangePasswordOnFirstLogin = changePasswordOnFirstLogin,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            // Fetch roles from ROLE_MASTER
            var roles = await rolesContext.RoleMasters
                .Where(r => request.Roles.Contains(r.ROLE_ID) && r.IS_LOCKED != "Y")
                .ToListAsync();

            user.Roles = roles.Select(r => new UserRole { RoleName = r.ROLE_DESCRIPTION }).ToList();

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> ChangeUserRoleAsync(string currentUserEmail, string email, List<string> newRoles)
        {
            if (currentUserEmail == email)
            {
                throw new InvalidOperationException("You cannot modify your own account");
            }

            var user = await context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            // Fetch roles from ROLE_MASTER
            var roles = await rolesContext.RoleMasters
                .Where(r => newRoles.Contains(r.ROLE_ID) && r.IS_LOCKED != "Y")
                .ToListAsync();

            user.Roles = roles.Select(r => new UserRole { RoleName = r.ROLE_DESCRIPTION }).ToList();

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user, string modifiedBy)
        {
            user.ModifiedBy = modifiedBy; // Set the modifier
            user.ModifiedAt = DateTime.UtcNow; // Set the modification time

            context.Users.Update(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<UserRole>> GetAllRolesAsync()
        {
            return await context.UserRoles
                .ToListAsync();
        }

        public async Task<List<string>> GetRoleDescriptionsAsync(List<string> roleIds)
        {
            return await context.UserRoles
                .Where(r => roleIds.Contains(r.Id.ToString()))
                .Select(r => r.RoleName)
                .ToListAsync();
        }

        public async Task<List<UserRole>> GetRolesByIdsAsync(List<string> roleIds)
        {
            return await context.UserRoles
                .Where(r => roleIds.Contains(r.Id.ToString()))
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(string currentUserEmail, string email)
        {
            if (currentUserEmail == email)
            {
                throw new InvalidOperationException("You cannot delete your own account");
            }

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
            var userRoles = user.Roles.Select(r => r.RoleName).ToList();

            // Fetch role descriptions from ROLE_MASTER
            var roleDescriptions = await rolesContext.RoleMasters
                .Where(r => userRoles.Contains(r.ROLE_ID))
                .Select(r => r.ROLE_DESCRIPTION)
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("ChangePasswordOnFirstLogin", user.ChangePasswordOnFirstLogin.ToString())
            };

            // Fix: Use RoleName instead of Id for ClaimTypes.Role
            var roleClaims = user.Roles.Select(r => new Claim(ClaimTypes.Role, r.RoleName)).ToList();
            claims.AddRange(roleClaims);

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

        public async Task<decimal?> GetRoleIdByNameAsync(string roleName)
        {
            var role = await rolesContext.RoleMasters.FirstOrDefaultAsync(r => r.ROLE_ID == roleName);
            return role?.SEQ_ROLE_MASTER;
        }

        public async Task<string?> GetRoleNameByIdAsync(int userRoleId)
        {
            var role = await rolesContext.RoleMasters.FirstOrDefaultAsync(r => r.SEQ_ROLE_MASTER == userRoleId);
            return role?.ROLE_ID;
        }
    }
}
