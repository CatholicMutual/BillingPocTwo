using System.Security.Claims;
using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Auth.Api.Services;
using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingPocTwo.Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserDbContext _context;

        public AuthController(IAuthService authService, UserDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("User already exists");
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto request)
        {
            var user = await _authService.RegisterAsync(request, true);
            if (user is null)
            {
                return BadRequest("User already exists");
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user-roles")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            var roles = await _context.UserRoles.ToListAsync();
            return Ok(roles);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user-roles/{email}")]
        public async Task<ActionResult<ChangeRoleDto>> GetUserRolesByEmail(string email)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userRoles = user.Roles.Select(r => r.Name).ToList();

            var result = new ChangeRoleDto
            {
                Email = email,
                NewRoles = userRoles
            };

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("search-users")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers([FromQuery] string email)
        {
            var users = await _context.Users
                .Where(u => u.Email.Contains(email))
                .ToListAsync();

            return Ok(users);
        }

        [Authorize]
        [HttpGet("user-profile/{email}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(string email)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userProfile = new UserProfileDto
            {
                Email = user.Email,
                UserId = user.Id,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };

            return Ok(userProfile);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("change-user-role")]
        public async Task<IActionResult> ChangeUserRole(ChangeRoleDto request)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _context.UserRoles.Where(r => request.NewRoles.Contains(r.Name)).ToListAsync();
            user.Roles = roles;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (currentUserEmail == null)
            {
                return Unauthorized();
            }

            if (currentUserEmail == email)
            {
                return BadRequest("You cannot delete your own account");
            }

            var result = await _authService.DeleterUserAsync(email);
            if (!result)
            {
                return NotFound("User not found");
            }

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            await _authService.LogoutAsync(Guid.Parse(userId));
            return NoContent();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshTokens(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid token");
            }
            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var passwordHasher = new PasswordHasher<User>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
            {
                return BadRequest("Current password is incorrect");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.NewPassword);
            user.PasswordHash = hashedPassword;
            user.ChangePasswordOnFirstLogin = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}