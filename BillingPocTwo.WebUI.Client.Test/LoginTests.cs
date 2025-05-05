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
using System.Net.Http;
using Blazored.SessionStorage;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class LoginTests : TestContext
    {
        private readonly Mock<ISessionStorageService> _sessionStorageMock;
        private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        public LoginTests()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>();
            var userState = new UserState();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            // Create an IHttpClientFactory mock to satisfy the constructor requirement
            var httpClient = CreateMockHttpClient();

            _httpClientFactoryMock
                .Setup(factory => factory.CreateClient("AuthApi"))
                .Returns(httpClient); // Setup the mock behavior

            var customAuthStateProvider = new CustomAuthenticationStateProvider(
                _httpClientFactoryMock.Object, // Pass the mocked IHttpClientFactory
                _sessionStorageMock.Object,
                userState,
                _httpClient
            );

            Services.AddSingleton(_httpClient);
            Services.AddSingleton(_httpClientFactoryMock.Object);
            Services.AddSingleton(_sessionStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);

            fakeNavigationManager.NavigateTo("https://localhost:7192/");
        }

        //[Fact]
        //public void Login_ShouldRender()
        //{
        //    // Arrange
        //    var cut = RenderComponent<Login>();

        //    // Act
        //    var loginForm = cut.Find("form");

        //    // Assert
        //    Assert.NotNull(loginForm);
        //}

        //[Fact]
        //public async Task Login_ShouldDisplayErrorMessage_WhenLoginFails()
        //{
        //    // Arrange
        //    var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };
        //    var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        //    {
        //        Content = new StringContent("Invalid username or password")
        //    };

        //    var failingClient = new HttpClient(_httpMessageHandlerMock.Object)
        //    {
        //        BaseAddress = new Uri("https://localhost:7192/")
        //    };

        //    _httpClientFactoryMock
        //        .Setup(factory => factory.CreateClient("AuthApi"))
        //        .Returns(failingClient);

        //    _httpMessageHandlerMock
        //        .Protected()
        //        .Setup<Task<HttpResponseMessage>>(
        //            "SendAsync",
        //            ItExpr.Is<HttpRequestMessage>(req =>
        //                req.Method == HttpMethod.Post &&
        //                req.RequestUri == new Uri("https://localhost:7192/api/auth/login")),
        //            ItExpr.IsAny<CancellationToken>()
        //        )
        //        .ReturnsAsync(responseMessage);

        //    var httpClient = CreateMockHttpClient();

        //    var cut = RenderComponent<Login>();
        //    cut.Instance.loginModel = loginDto;

        //    // Act
        //    await cut.InvokeAsync(() => cut.Instance.HandleLogin());

        //    // Assert
        //    var errorMessage = cut.WaitForElement("em", TimeSpan.FromSeconds(15));
        //    errorMessage.MarkupMatches("<em>Invalid username or password</em>");
        //}

        //[Fact]
        //public async Task Login_ShouldNavigateToWelcomePage_WhenLoginSucceeds()
        //{
        //    // Arrange
        //    var cut = RenderComponent<Login>();
        //    var loginDto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
        //    var tokenResponse = new TokenResponseDto { AccessToken = GenerateValidJwtToken(), RefreshToken = "validRefreshToken" };
        //    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = JsonContent.Create(tokenResponse)
        //    };
        //    var httpClient = CreateMockHttpClient();


        //    var expectedUri = new Uri("https://localhost:7192/api/auth/login");

        //    _httpClientFactoryMock
        //       .Setup(factory => factory.CreateClient("AuthApi"))
        //       .Returns(httpClient); // Now mock the named client for "AuthApi"

        //    // Act
        //    await cut.InvokeAsync(() => cut.Instance.HandleLogin());

        //    // Assert
        //    cut.WaitForState(() => !cut.Instance.isLoading);
        //    _sessionStorageMock.Verify(x =>
        //        x.SetItemAsync("AccessToken", It.Is<string>(token => !string.IsNullOrWhiteSpace(token)), CancellationToken.None),
        //        Times.Once);
        //    var customAuthStateProvider = Services.GetRequiredService<AuthenticationStateProvider>() as CustomAuthenticationStateProvider;
        //    Assert.NotNull(customAuthStateProvider);
        //    customAuthStateProvider.NotifyUserAuthentication(tokenResponse.AccessToken);
        //    var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
        //    Assert.NotNull(navigationManager);
        //    Assert.Equal("https://localhost:7192/welcome2", navigationManager.Uri);
        //}

        private string GenerateValidJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-secret-your-256-bit-secret");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),  // User ID
                    new Claim(ClaimTypes.Email, "test@example.com"), // Email
                    new Claim(ClaimTypes.Name, "Test User"), // Name (Optional)
                    new Claim("role", "User") // Example of an additional claim (e.g., role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static HttpClient CreateMockHttpClientRender()
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}") // simulate a successful response
                });

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };
        }

        private HttpClient CreateMockHttpClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = GenerateValidJwtToken(),
                RefreshToken = "validRefreshToken"
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/login")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };
        }

    }
}
