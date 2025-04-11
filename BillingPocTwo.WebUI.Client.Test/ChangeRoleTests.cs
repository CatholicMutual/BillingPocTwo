using Bunit;
using BillingPocTwo.WebUI.Client.Pages.Admin;
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
    public class ChangeRoleTests : TestContext
    {
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public ChangeRoleTests()
        {
            _localStorageMock = new Mock<ILocalStorageService>();
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
                _localStorageMock.Object,
                userState,
                _httpClient);

            Services.AddSingleton<HttpClient>(_httpClient);
            Services.AddSingleton<IHttpClientFactory>(httpClientFactoryMock.Object);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
        }

        [Fact]
        public void ChangeRole_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<ChangeRole>();

            // Act
            var form = cut.Find("form");

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public async Task ChangeRole_ShouldDisplayErrorMessage_WhenUserSearchFails()
        {
            // Arrange
            var userEmail = "test@example.com";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("User not found")
            };

            // Mock the response for loading user roles
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-roles")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Roles not found")
                });

            var cut = RenderComponent<ChangeRole>();
            cut.Instance.userEmail = userEmail;

            // Act
            await cut.InvokeAsync(() => cut.Instance.SearchUser());

            // Assert
            cut.WaitForState(() => !cut.Instance.isLoading);
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Failed to load user roles", errorMessage);
            });
        }

        [Fact]
        public async Task ChangeRole_ShouldChangeUserRole_WhenChangeRoleSucceeds()
        {
            // Arrange
            var cut = RenderComponent<ChangeRole>();
            var changeRoleDto = new ChangeRoleDto { Email = "test@example.com", NewRoles = new List<string> { "Admin" } };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(changeRoleDto)
            };

            var expectedUri = new Uri("https://localhost:7192/api/auth/change-user-details");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleChangeRole());

            // Assert
            cut.WaitForState(() => !cut.Instance.isLoading);
            cut.WaitForAssertion(() =>
            {
                var successMessage = cut.Find("em").TextContent;
                Assert.Equal("Details changed successfully", successMessage);
            });
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

