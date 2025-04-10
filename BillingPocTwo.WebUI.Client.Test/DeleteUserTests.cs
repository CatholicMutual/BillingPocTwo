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
    public class DeleteUserTests : TestContext
    {
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public DeleteUserTests()
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

            Services.AddSingleton<HttpClient>(_httpClient);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
        }

        [Fact]
        public void DeleteUser_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<DeleteUser>();

            // Act
            var form = cut.Find("form");

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public async Task DeleteUser_ShouldDisplayErrorMessage_WhenUserNotAuthenticated()
        {
            // Arrange
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null); // No token

            var cut = RenderComponent<DeleteUser>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleDeleteUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("User is not authenticated", errorMessage);
            });
        }

        [Fact]
        public async Task DeleteUser_ShouldDisplayErrorMessage_WhenDeletingOwnAccount()
        {
            // Arrange
            var token = GenerateValidJwtToken("admin@example.com");
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            // Mocking the HttpClient to return a response
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/delete-user/admin@example.com")), // Adjust to match the actual endpoint
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) // Return failure for deletion
                {
                    Content = new StringContent("You cannot delete your own account.")
                });

            var cut = RenderComponent<DeleteUser>();
            cut.Instance.deleteUserModel.Email = "admin@example.com"; // Same as the logged-in user

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleDeleteUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("You cannot delete your own account.", errorMessage);
            });
        }

        [Fact]
        public async Task DeleteUser_ShouldDeleteUserSuccessfully()
        {
            // Arrange
            var token = GenerateValidJwtToken("admin@example.com");
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/delete-user/test@example.com")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var cut = RenderComponent<DeleteUser>();
            cut.Instance.deleteUserModel.Email = "test@example.com";

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleDeleteUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var successMessage = cut.Find("em").TextContent;
                Assert.Equal("User deleted successfully", successMessage);
            });
        }

        [Fact]
        public async Task DeleteUser_ShouldDisplayErrorMessage_WhenDeletionFails()
        {
            // Arrange
            var token = GenerateValidJwtToken("admin@example.com");
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/delete-user/test@example.com")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Deletion failed")
                });

            var cut = RenderComponent<DeleteUser>();
            cut.Instance.deleteUserModel.Email = "test@example.com";

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleDeleteUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Deletion failed", errorMessage);
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
    }
}
