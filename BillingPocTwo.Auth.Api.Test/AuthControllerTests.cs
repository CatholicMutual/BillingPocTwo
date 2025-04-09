using BillingPocTwo.Auth.Api.Controllers;
using BillingPocTwo.Auth.Api.Data;
using BillingPocTwo.Auth.Api.Services;
using BillingPocTwo.Shared.DataObjects;
using BillingPocTwo.Shared.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace BillingPocTwo.Auth.Api.Test
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<DbSet<User>> _usersDbSetMock;
        private readonly Mock<IUserDbContext> _contextMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            var users = new List<User>
                {
                    new User { Email = "test@example.com", FirstName = "John", LastName = "Doe" }
                }.AsQueryable().BuildMockDbSet();
            _usersDbSetMock = users;
            _contextMock = new Mock<IUserDbContext>();
            _contextMock.Setup(c => c.Users).Returns(_usersDbSetMock.Object);
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
            _controller = new AuthController(_authServiceMock.Object, _contextMock.Object);
        }

        [Fact]
        public async Task CreateUser_ReturnsOkResult_WhenUserIsCreated()
        {
            // Arrange
            var createUserDto = new CreateUserDto { Email = "test@example.com" };
            var user = new User { Email = "test@example.com" };
            _authServiceMock.Setup(s => s.RegisterAsync(createUserDto, true)).ReturnsAsync(user);
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "admin@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUserIsUpdated()
        {
            // Arrange
            var userDto = new UserDto { Email = "test@example.com", FirstName = "John", LastName = "Doe" };
            var user = new User { Email = "test@example.com" };
            _usersDbSetMock.Setup(m => m.FindAsync(It.IsAny<object[]>()))
               .Returns(new ValueTask<User>(user));
            _authServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<User>(), "admin@example.com"))
                .ReturnsAsync(true);
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "admin@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.UpdateUser(userDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetUserRoles_ReturnsOkResult_WithListOfUserRoles()
        {
            // Arrange
            var userRoles = new List<UserRole>
            {
                new UserRole { Id = 1, Name = "Admin" },
                new UserRole { Id = 2, Name = "User" }
            }.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(c => c.UserRoles).Returns(userRoles.Object);

            // Act
            var result = await _controller.GetUserRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<UserRole>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetUserRolesByEmail_ReturnsOkResult_WithUserRoles()
        {
            // Arrange
            var userEmail = "test@example.com";
            var roles = new List<UserRole>
            {
                new UserRole { Id = 1, Name = "Admin" },
                new UserRole { Id = 2, Name = "User" }
            };

            var user = new User
            {
                Email = userEmail,
                FirstName = "Jane",
                LastName = "Doe",
                Active = true,
                ServiceUser = false,
                Roles = roles
            };

            var users = new List<User> { user }.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(c => c.Users).Returns(users.Object);

            // Act
            var result = await _controller.GetUserRolesByEmail(userEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ChangeRoleDto>(okResult.Value);
            Assert.Equal(userEmail, dto.Email);
            Assert.Equal("Jane", dto.FirstName);
            Assert.Equal("Doe", dto.LastName);
            Assert.Equal(2, dto.NewRoles.Count);
            Assert.Contains("Admin", dto.NewRoles);
            Assert.Contains("User", dto.NewRoles);
            Assert.True(dto.Active);
            Assert.False(dto.ServiceUser);
        }

        [Fact]
        public async Task SearchUsers_ReturnsOkResult_WithMatchingUsers()
        {
            // Arrange
            var usersList = new List<User>
            {
                new User { Email = "john.doe@example.com", FirstName = "John", LastName = "Doe" },
                new User { Email = "jane.doe@example.com", FirstName = "Jane", LastName = "Doe" },
                new User { Email = "other.user@example.com", FirstName = "Other", LastName = "User" }
            };

            var mockUsersDbSet = usersList.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            var searchQuery = "doe";

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<User>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.All(returnValue, u => Assert.Contains("doe", u.Email));
        }

        [Fact]
        public async Task GetUserProfile_ReturnsOkResult_WithUserProfile()
        {
            // Arrange
            var userEmail = "john.doe@example.com";
            var roles = new List<UserRole>
            {
                new UserRole { Id = 1, Name = "Admin" },
                new UserRole { Id = 2, Name = "User" }
            };

            var user = new User
            {
                Email = userEmail,
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Active = true,
                ServiceUser = false,
                Roles = roles
            };

            var users = new List<User> { user }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Users).Returns(users.Object);

            // Act
            var result = await _controller.GetUserProfile(userEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userProfileDto = Assert.IsType<UserProfileDto>(okResult.Value);
            Assert.Equal(userEmail, userProfileDto.Email);
            Assert.Equal(user.Id, userProfileDto.UserId);
            Assert.Equal("John", userProfileDto.FirstName);
            Assert.Equal("Doe", userProfileDto.LastName);
            Assert.True(userProfileDto.Active);
            Assert.False(userProfileDto.ServiceUser);
            Assert.Equal(2, userProfileDto.Roles.Count);
            Assert.Contains("Admin", userProfileDto.Roles);
            Assert.Contains("User", userProfileDto.Roles);
        }

        [Fact]
        public async Task GetUserProfile_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var users = new List<User>().AsQueryable().BuildMockDbSet(); // Empty set
            _contextMock.Setup(c => c.Users).Returns(users.Object);

            // Act
            var result = await _controller.GetUserProfile("notfound@example.com");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task ChangeUserDetails_ReturnsNoContent_WhenDetailsAreChangedSuccessfully()
        {
            // Arrange
            var currentUserEmail = "admin@example.com";
            var request = new ChangeRoleDto
            {
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                Active = true,
                ServiceUser = false,
                NewRoles = new List<string> { "User" }
            };

            var roles = new List<UserRole>
            {
                new UserRole { Name = "User", Id = 1 }
            };

            var user = new User
            {
                Email = "user@example.com",
                FirstName = "OldName",
                LastName = "OldLastName",
                Active = true,
                ServiceUser = false,
                Roles = new List<UserRole>(),
                ModifiedBy = null,
                ModifiedAt = null
            };

            // Mock the context and DbSets
            var mockUsersDbSet = new List<User> { user }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            var mockRolesDbSet = roles.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.UserRoles).Returns(mockRolesDbSet.Object);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.ChangeUserDetails(request);

            // Assert
            Assert.IsType<NoContentResult>(result);  // Expect NoContentResult
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);  // Ensure SaveChangesAsync was called
            Assert.Equal(request.FirstName, user.FirstName);  // Verify changes
            Assert.Equal(request.LastName, user.LastName);    // Verify changes
            Assert.Equal(request.Email, user.Email);          // Verify changes
            Assert.Equal(request.Active, user.Active);        // Verify changes
            Assert.Equal(request.ServiceUser, user.ServiceUser);  // Verify changes
            Assert.Contains(roles[0], user.Roles);  // Verify the role is assigned
        }

        [Fact]
        public async Task ChangeUserDetails_ReturnsBadRequest_WhenAttemptingToModifyOwnAccount()
        {
            // Arrange
            var currentUserEmail = "user@example.com";  // Same email as the request
            var request = new ChangeRoleDto
            {
                Email = currentUserEmail,  // Same email
                FirstName = "NewFirstName",
                LastName = "NewLastName",
                Active = true,
                ServiceUser = false,
                NewRoles = new List<string> { "Admin" }
            };

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.ChangeUserDetails(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You cannot modify your own account", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeUserDetails_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var currentUserEmail = "admin@example.com";
            var request = new ChangeRoleDto
            {
                Email = "nonexistent@example.com",  // Non-existent user
                FirstName = "John",
                LastName = "Doe",
                Active = true,
                ServiceUser = false,
                NewRoles = new List<string> { "User" }
            };

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Mock an empty user list
            var mockUsersDbSet = new List<User>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            // Act
            var result = await _controller.ChangeUserDetails(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            var currentUserEmail = "admin@example.com";
            var userEmailToDelete = "user@example.com";

            // Mock the service method to return true, indicating successful deletion
            _authServiceMock.Setup(s => s.DeleteUserAsync(currentUserEmail, userEmailToDelete)).ReturnsAsync(true);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.DeleteUser(userEmailToDelete);

            // Assert
            Assert.IsType<NoContentResult>(result);  // Expect NoContentResult
            _authServiceMock.Verify(s => s.DeleteUserAsync(currentUserEmail, userEmailToDelete), Times.Once);  // Ensure DeleteUserAsync was called
        }

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenAttemptingToDeleteOwnAccount()
        {
            // Arrange
            var currentUserEmail = "user@example.com";  // Trying to delete own account
            var userEmailToDelete = currentUserEmail;

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.DeleteUser(userEmailToDelete);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You cannot delete your own account", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var currentUserEmail = "admin@example.com";
            var userEmailToDelete = "nonexistent@example.com";  // Non-existent user

            // Mock the service method to return false, indicating the user was not found
            _authServiceMock.Setup(s => s.DeleteUserAsync(currentUserEmail, userEmailToDelete)).ReturnsAsync(false);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, currentUserEmail) }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            // Act
            var result = await _controller.DeleteUser(userEmailToDelete);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsOkResult_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "validUser@example.com",
                Password = "validPassword"
            };

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = "validToken",
                RefreshToken = "validRefreshToken"
            };

            _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(tokenResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TokenResponseDto>(okResult.Value);
            Assert.Equal("validToken", returnValue.AccessToken);
            Assert.Equal("validRefreshToken", returnValue.RefreshToken);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "invalidUser@example.com",
                Password = "invalidPassword"
            };

            _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((TokenResponseDto)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Logout_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Logout_ReturnsNoContent_WhenUserIdIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            _authServiceMock.Verify(s => s.LogoutAsync(Guid.Parse(userId)), Times.Once);
        }

        [Fact]
        public async Task RefreshTokens_ReturnsOkResult_WhenTokensAreValid()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequestDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "validRefreshToken"
            };

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = "newAccessToken",
                RefreshToken = "newRefreshToken"
            };

            _authServiceMock.Setup(s => s.RefreshTokensAsync(refreshTokenRequest)).ReturnsAsync(tokenResponse);

            // Act
            var result = await _controller.RefreshTokens(refreshTokenRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TokenResponseDto>(okResult.Value);
            Assert.Equal("newAccessToken", returnValue.AccessToken);
            Assert.Equal("newRefreshToken", returnValue.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokens_ReturnsUnauthorized_WhenTokensAreInvalid()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequestDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "invalidRefreshToken"
            };

            _authServiceMock.Setup(s => s.RefreshTokensAsync(refreshTokenRequest)).ReturnsAsync((TokenResponseDto)null);

            // Act
            var result = await _controller.RefreshTokens(refreshTokenRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid token", unauthorizedResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenUserEmailIsNull()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var request = new ChangePasswordDto { Email = "test@example.com", CurrentPassword = "oldPassword", NewPassword = "newPassword", ConfirmPassword = "newPassword" };

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "test@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var request = new ChangePasswordDto { Email = "test@example.com", CurrentPassword = "oldPassword", NewPassword = "newPassword", ConfirmPassword = "newPassword" };
            _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var userEmail = "test@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, userEmail) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var request = new ChangePasswordDto { Email = userEmail, CurrentPassword = "wrongPassword", NewPassword = "newPassword", ConfirmPassword = "newPassword" };
            var user = new User { Email = userEmail, PasswordHash = new PasswordHasher<User>().HashPassword(null, "oldPassword") };
            _contextMock.Setup(c => c.Users).Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Current password is incorrect", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var userEmail = "test@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, userEmail) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var request = new ChangePasswordDto { Email = userEmail, CurrentPassword = "oldPassword", NewPassword = "newPassword", ConfirmPassword = "differentPassword" };
            var user = new User { Email = userEmail, PasswordHash = new PasswordHasher<User>().HashPassword(null, "oldPassword") };
            _contextMock.Setup(c => c.Users).Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Passwords do not match", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ReturnsNoContent_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var userEmail = "test@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, userEmail) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            var request = new ChangePasswordDto { Email = userEmail, CurrentPassword = "oldPassword", NewPassword = "newPassword", ConfirmPassword = "newPassword" };
            var user = new User { Email = userEmail, PasswordHash = new PasswordHasher<User>().HashPassword(null, "oldPassword") };
            _contextMock.Setup(c => c.Users).Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
