using Bunit;
using BillingPocTwo.WebUI.Client.Pages;
using BillingPocTwo.WebUI.Client.Test.Helpers;
using BillingPocTwo.Shared.DataObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using BillingPocTwo.WebUI.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class LoginTests : TestContext
    {
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public LoginTests()
        {
            _localStorageMock = new Mock<ILocalStorageService>();
            _authStateProviderMock = new Mock<AuthenticationStateProvider>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            Services.AddSingleton(_httpClient);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton(_authStateProviderMock.Object);
            Services.AddAuthorizationCore();
            Services.AddScoped<AuthenticationStateProvider, TestAuthenticationStateProvider>();
        }

        [Fact]
        public void Login_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<Login>();

            // Act
            var loginForm = cut.Find("form");

            // Assert
            Assert.NotNull(loginForm);
        }

        [Fact]
        public async Task Login_ShouldDisplayErrorMessage_WhenLoginFails()
        {
            // Arrange
            var cut = RenderComponent<Login>();
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Invalid username or password")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("https://localhost:7192/api/auth/login")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            await cut.Instance.HandleLogin();

            // Assert
            cut.WaitForState(() => !cut.Instance.isLoading);
            await Task.Delay(1000); // Add a delay to ensure the state has been updated

            var errorMessage = cut.Find("em").TextContent;
            Console.WriteLine($"Error message: {errorMessage}"); // Add logging to debug the issue
            Assert.Equal("Invalid username or password", errorMessage);
        }

        [Fact]
        public async Task Login_ShouldNavigateToWelcomePage_WhenLoginSucceeds()
        {
            // Arrange
            var cut = RenderComponent<Login>();
            var loginDto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
            var tokenResponse = new TokenResponseDto { AccessToken = GenerateValidJwtToken(), RefreshToken = "validRefreshToken" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };

            var expectedUri = new Uri("https://localhost:7192/api/auth/login");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            await cut.Instance.HandleLogin();

            // Assert
            cut.WaitForState(() => !cut.Instance.isLoading);
            _localStorageMock.Verify(x => x.SetItemAsync("authToken", tokenResponse.AccessToken, CancellationToken.None), Times.Once);
            _authStateProviderMock.Verify(x => ((CustomAuthenticationStateProvider)x).NotifyUserAuthentication(tokenResponse.AccessToken), Times.Once);
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            Assert.Equal("https://localhost:7192/welcome2", navigationManager.Uri);
        }

        private string GenerateValidJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-secret");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
