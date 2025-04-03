using BillingPocTwo.Auth.Api.Controllers;
using BillingPocTwo.Auth.Api.Services;
using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using BillingPocTwo.Auth.Api.Test;

namespace BillingPocTwo.Auth.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<TestUserDbContext> _contextMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            var users = new List<User>().AsQueryable().BuildMockDbSet();
            _contextMock = new Mock<TestUserDbContext>(new DbContextOptions<UserDbContext>(), users.Object);
            _controller = new AuthController(_authServiceMock.Object, _contextMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateUser_ReturnsOkResult_WhenUserIsCreated()
        {
            // Arrange
            var request = new CreateUserDto { Email = "newuser@example.com" };
            var user = new User { Email = "newuser@example.com" };
            _authServiceMock.Setup(s => s.RegisterAsync(request, true)).ReturnsAsync(user);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            var request = new CreateUserDto { Email = "existinguser@example.com" };
            _authServiceMock.Setup(s => s.RegisterAsync(request, true)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.CreateUser(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUserIsUpdated()
        {
            // Arrange
            var request = new UserDto { Email = "user@example.com", FirstName = "John", LastName = "Doe" };
            var user = new User { Email = "user@example.com" };
            _contextMock.Setup(c => c.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default)).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.UpdateUserAsync(user, "admin@example.com")).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new UserDto { Email = "nonexistentuser@example.com" };
            _contextMock.Setup(c => c.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.UpdateUser(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetUserRoles_ReturnsOkResult_WithUserRoles()
        {
            // Arrange
            var roles = new List<UserRole> { new UserRole { Name = "Admin" } };
            _contextMock.Setup(c => c.UserRoles.ToListAsync(default)).ReturnsAsync(roles);

            // Act
            var result = await _controller.GetUserRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(roles, okResult.Value);
        }

        [Fact]
        public async Task GetUserRolesByEmail_ReturnsOkResult_WithUserRoles()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User { Email = email, Roles = new List<UserRole> { new UserRole { Name = "Admin" } } };
            _contextMock.Setup(c => c.Users.Include(u => u.Roles).FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserRolesByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultDto = Assert.IsType<ChangeRoleDto>(okResult.Value);
            Assert.Equal(email, resultDto.Email);
            Assert.Contains("Admin", resultDto.NewRoles);
        }

        [Fact]
        public async Task SearchUsers_ReturnsOkResult_WithMatchingUsers()
        {
            // Arrange
            var email = "user";
            var users = new List<User> { new User { Email = "user@example.com" } };
            _contextMock.Setup(c => c.Users.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()).ToListAsync(default)).ReturnsAsync(users);

            // Act
            var result = await _controller.SearchUsers(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task GetUserProfile_ReturnsOkResult_WithUserProfile()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User { Email = email, Roles = new List<UserRole> { new UserRole { Name = "Admin" } } };
            _contextMock.Setup(c => c.Users.Include(u => u.Roles).FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserProfile(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userProfile = Assert.IsType<UserProfileDto>(okResult.Value);
            Assert.Equal(email, userProfile.Email);
            Assert.Contains("Admin", userProfile.Roles);
        }

        [Fact]
        public async Task ChangeUserDetails_ReturnsNoContent_WhenUserDetailsAreChanged()
        {
            // Arrange
            var request = new ChangeRoleDto { Email = "user@example.com", FirstName = "John", LastName = "Doe", NewRoles = new List<string> { "Admin" } };
            var user = new User { Email = "user@example.com", Roles = new List<UserRole> { new UserRole { Name = "User" } } };
            var roles = new List<UserRole> { new UserRole { Name = "Admin" } };
            _contextMock.Setup(c => c.Users.Include(u => u.Roles).FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(), default)).ReturnsAsync(user);
            _contextMock.Setup(c => c.UserRoles.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<UserRole, bool>>>()).ToListAsync(default)).ReturnsAsync(roles);

            // Act
            var result = await _controller.ChangeUserDetails(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserIsDeleted()
        {
            // Arrange
            var email = "user@example.com";
            _authServiceMock.Setup(s => s.DeleteUserAsync("admin@example.com", email)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(email);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOkResult_WithTokenResponse()
        {
            // Arrange
            var request = new LoginDto { Email = "user@example.com", Password = "password" };
            var tokenResponse = new TokenResponseDto { AccessToken = "access_token", RefreshToken = "refresh_token" };
            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(tokenResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tokenResponse, okResult.Value);
        }

        [Fact]
        public async Task Logout_ReturnsNoContent_WhenUserIsLoggedOut()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RefreshTokens_ReturnsOkResult_WithTokenResponse()
        {
            // Arrange
            var request = new RefreshTokenRequestDto { UserId = Guid.NewGuid(), RefreshToken = "refresh_token" };
            var tokenResponse = new TokenResponseDto { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" };
            _authServiceMock.Setup(s => s.RefreshTokensAsync(request)).ReturnsAsync(tokenResponse);

            // Act
            var result = await _controller.RefreshTokens(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(tokenResponse, okResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ReturnsNoContent_WhenPasswordIsChanged()
        {
            // Arrange
            var request = new ChangePasswordDto { CurrentPassword = "current_password", NewPassword = "new_password", ConfirmPassword = "new_password" };
            var user = new User { Email = "admin@example.com", PasswordHash = new PasswordHasher<User>().HashPassword(null, "current_password") };
            var users = new List<User> { user }.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(c => c.Users).Returns(users.Object);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
