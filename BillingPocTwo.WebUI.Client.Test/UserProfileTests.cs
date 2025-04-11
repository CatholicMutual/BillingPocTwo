using Bunit;
using BillingPocTwo.WebUI.Client.Pages.Users.Account;
using BillingPocTwo.Shared.DataObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using BillingPocTwo.Shared.Entities;
using BillingPocTwo.WebUI.Client.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using BillingPocTwo.Shared.Entities.Auth;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class UserProfileTests : TestContext
    {
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public UserProfileTests()
        {
            _localStorageMock = new Mock<ILocalStorageService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            var userState = new UserState();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var customAuthStateProvider = new CustomAuthenticationStateProvider(httpClientFactoryMock.Object, _localStorageMock.Object, userState, _httpClient);
            var fakeAuthStateProvider = new Mock<AuthenticationStateProvider>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            }, "mock"));

            Services.AddSingleton<HttpClient>(_httpClient);
            Services.AddSingleton<IHttpClientFactory>(httpClientFactoryMock.Object);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            fakeAuthStateProvider
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(new AuthenticationState(claimsPrincipal));

            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
            Services.AddSingleton<AuthenticationStateProvider>(fakeAuthStateProvider.Object);
        }

        [Fact]
        public void UserProfile_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<UserProfile>();

            // Act
            var heading = cut.Find("h3");

            // Assert
            Assert.Equal("User Profile", heading.TextContent);
        }

        [Fact]
        public async Task UserProfile_ShouldDisplayErrorMessage_WhenUserNotAuthenticated()
        {
            // Arrange
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null); // No token

            var cut = RenderComponent<UserProfile>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.LoadUserProfile());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("p").TextContent;
                Assert.Equal("User is not authenticated", errorMessage);
            });
        }

        [Fact]
        public async Task UserProfile_ShouldDisplayErrorMessage_WhenEmailClaimIsMissing()
        {
            // Arrange
            var token = GenerateJwtTokenWithoutEmail();
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            // Optionally prevent the HTTP call by mocking a blank user state (simulate no email in claims)
            var fakeAuthStateProvider = new Mock<AuthenticationStateProvider>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // No email claim

            fakeAuthStateProvider
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(new AuthenticationState(claimsPrincipal));

            Services.AddSingleton<AuthenticationStateProvider>(fakeAuthStateProvider.Object);

            // This mock ensures that if the HTTP call is accidentally made, we return a safe response
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var cut = RenderComponent<UserProfile>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.LoadUserProfile());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("p").TextContent;
                Assert.Equal("Failed to retrieve user email", errorMessage);
            });
        }


        [Fact]
        public async Task UserProfile_ShouldLoadUserProfileSuccessfully()
        {
            // Arrange
            var token = GenerateValidJwtToken("user@example.com");
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var userProfile = new UserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "user@example.com",
                UserId = Guid.NewGuid(),
                Active = true,
                ServiceUser = false,
                Roles = new List<string> { "Admin", "User" }
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-profile/user@example.com")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    var json = JsonSerializer.Serialize(userProfile);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = content
                    };
                });

            var cut = RenderComponent<UserProfile>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.LoadUserProfile());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var firstName = cut.Find("p strong").TextContent;
                Assert.Equal("First Name:", firstName);
                Assert.Contains("John", cut.Markup);
                Assert.Contains("Admin", cut.Markup);
            });
        }

        [Fact]
        public async Task UserProfile_ShouldDisplayErrorMessage_WhenProfileLoadFails()
        {
            // Arrange
            var token = GenerateValidJwtToken("user@example.com");
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var fakeAuthStateProvider = new Mock<AuthenticationStateProvider>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "user@example.com")
            }, "mock"));

            fakeAuthStateProvider
                .Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(new AuthenticationState(claimsPrincipal));

            Services.AddSingleton<AuthenticationStateProvider>(fakeAuthStateProvider.Object);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-profile/user@example.com")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var cut = RenderComponent<UserProfile>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.LoadUserProfile());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("p").TextContent;
                Assert.Equal("Failed to load user profile", errorMessage);
            });
        }

        private string GenerateValidJwtToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-secret-your-256-bit-secret");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateJwtTokenWithoutEmail()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-secret-your-256-bit-secret");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
