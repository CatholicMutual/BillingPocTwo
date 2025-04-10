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
using BillingPocTwo.Shared.Entities;
using Bunit.TestDoubles;
using BillingPocTwo.Shared.Entities.Auth;

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
            var userState = new UserState();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            // Create an IHttpClientFactory mock to satisfy the constructor requirement
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var customAuthStateProvider = new CustomAuthenticationStateProvider(
                httpClientFactoryMock.Object, // Pass the mocked IHttpClientFactory
                _localStorageMock.Object,
                userState,
                _httpClient
            );

            Services.AddSingleton(_httpClient);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);

            fakeNavigationManager.NavigateTo("https://localhost:7192/");
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
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Invalid username or password")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/login")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192")
            };

            var cut = RenderComponent<Login>();
            cut.Instance.loginModel = loginDto;

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleLogin());

            // Assert
            cut.WaitForState(() => !cut.Instance.isLoading);
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Invalid username or password", errorMessage);
            });
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
            var customAuthStateProvider = Services.GetRequiredService<AuthenticationStateProvider>() as CustomAuthenticationStateProvider;
            Assert.NotNull(customAuthStateProvider);
            customAuthStateProvider.NotifyUserAuthentication(tokenResponse.AccessToken);
            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            Assert.NotNull(navigationManager);
            Assert.Equal("https://localhost:7192/welcome2", navigationManager.Uri);
        }

        private string GenerateValidJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-secret-your-256-bit-secret");
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
