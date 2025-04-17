using Bunit;
using BillingPocTwo.WebUI.Client.Pages.Users.Account;
using BillingPocTwo.Shared.DataObjects;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BillingPocTwo.Shared.Entities.Auth;
using BillingPocTwo.WebUI.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class ChangePasswordTests : TestContext
    {
        private readonly Mock<ISessionStorageService> _sessionStorageMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public ChangePasswordTests()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            // ? Create HttpClient using the already initialized _httpMessageHandlerMock
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            var userState = new UserState();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                                 .Returns(_httpClient);

            var customAuthStateProvider = new CustomAuthenticationStateProvider(
                httpClientFactoryMock.Object,
                _sessionStorageMock.Object,
                userState,
                _httpClient);

            Services.AddSingleton<HttpClient>(_httpClient);
            Services.AddSingleton<IHttpClientFactory>(httpClientFactoryMock.Object);
            Services.AddSingleton(_sessionStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
        }

        [Fact]
        public void ChangePassword_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<ChangePassword>();

            // Act
            var form = cut.Find("form");

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public async Task ChangePassword_ShouldDisplayErrorMessage_WhenUserNotAuthenticated()
        {
            // Arrange
            _sessionStorageMock
                .Setup(x => x.GetItemAsync<string>("AccessToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null); // No token

            var cut = RenderComponent<ChangePassword>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleChangePassword());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("User is not authenticated", errorMessage);
            });
        }

        [Fact]
        public async Task ChangePassword_ShouldDisplayErrorMessage_WhenEmailClaimIsMissing()
        {
            // Arrange
            var token = GenerateJwtTokenWithoutEmail();
            _sessionStorageMock
                .Setup(x => x.GetItemAsync<string>("AccessToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var cut = RenderComponent<ChangePassword>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleChangePassword());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Failed to retrieve user email from token", errorMessage);
            });
        }

        [Fact]
        public async Task ChangePassword_ShouldChangePasswordSuccessfully()
        {
            // Arrange
            var token = GenerateValidJwtToken("user@example.com");
            _sessionStorageMock
                .Setup(x => x.GetItemAsync<string>("AccessToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            // Ensure mock handler correctly intercepts the request
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/change-password")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var cut = RenderComponent<ChangePassword>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleChangePassword());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var successMessage = cut.Find("em").TextContent;
                Assert.Equal("Password changed successfully", successMessage);
            });
        }

        [Fact]
        public async Task ChangePassword_ShouldDisplayErrorMessage_WhenChangeFails()
        {
            // Arrange
            var token = GenerateValidJwtToken("user@example.com");
            _sessionStorageMock
                .Setup(x => x.GetItemAsync<string>("AccessToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/change-password")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Change failed")
                });

            var cut = RenderComponent<ChangePassword>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleChangePassword());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Failed to change password: Change failed", errorMessage);
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
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", email)
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
