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
    public class CreateUserTests : TestContext
    {
        private readonly Mock<ILocalStorageService> _localStorageMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public CreateUserTests()
        {
            _localStorageMock = new Mock<ILocalStorageService>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            var userState = new UserState();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            var customAuthStateProvider = new CustomAuthenticationStateProvider(httpClientFactoryMock.Object, _localStorageMock.Object, userState, _httpClient);

            Services.AddSingleton<HttpClient>(_httpClient);
            Services.AddSingleton<IHttpClientFactory>(httpClientFactoryMock.Object);
            Services.AddSingleton(_localStorageMock.Object);
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
        }

        private static HttpClient CreateMockHttpClient()
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
                    Content = new StringContent("{}") // Simulate a successful response
                });

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };
        }

        [Fact]
        public void CreateUser_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<CreateUser>();

            // Act
            var form = cut.Find("form");

            // Assert
            Assert.NotNull(form);
        }

        [Fact]
        public async Task CreateUser_ShouldLoadRolesSuccessfully()
        {
            // Arrange
            var roles = new List<UserRole>
            {
                new UserRole { Name = "Admin" },
                new UserRole { Name = "User" }
            };

            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync("fake-jwt-token");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-roles")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    Console.WriteLine($"Request Method: {req.Method}");
                    Console.WriteLine($"Request URI: {req.RequestUri}");
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(roles)
                });

            // Act
            var cut = RenderComponent<CreateUser>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Equal(roles.Count, cut.Instance.availableRoles.Count);
                Assert.Contains(cut.Instance.availableRoles, r => r.Name == "Admin");
                Assert.Contains(cut.Instance.availableRoles, r => r.Name == "User");
            });
        }

        [Fact]
        public async Task CreateUser_ShouldDisplayErrorMessage_WhenRolesFailToLoad()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-roles")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Failed to load roles")
                });

            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync("mock-token");

            // Act
            var cut = RenderComponent<CreateUser>();

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Failed to load roles", errorMessage);
            });
        }

        [Fact]
        public async Task CreateUser_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var roles = new List<UserRole>
            {
                new UserRole { Name = "Admin" },
                new UserRole { Name = "User" }
            };

            // Mock the response for loading roles
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-roles")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(roles)
                });

            // Mock the authentication token
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync("mock-token");

            var createUserDto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "password123",
                Roles = new List<string> { "Admin" }
            };

            // Mock the response for creating a user
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/create-user")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("User created successfully")
                });

            var cut = RenderComponent<CreateUser>();

            // Set the user model
            cut.Instance.createUserModel = createUserDto;

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleCreateUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var successMessage = cut.Find("em").TextContent;
                Assert.Equal("User created successfully", successMessage);
            });
        }

        [Fact]
        public async Task CreateUser_ShouldDisplayErrorMessage_WhenUserCreationFails()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "password123",
                Roles = new List<string> { "Admin" }
            };

            // Mock token so component can proceed
            _localStorageMock
                .Setup(x => x.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
                .ReturnsAsync("mock-token");

            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Invalid user data")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/create-user")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Load roles mock (since LoadRoles runs in OnInitializedAsync)
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri("https://localhost:7192/api/auth/user-roles")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new List<UserRole>
                    {
                new UserRole { Name = "Admin" }
                    })
                });

            var cut = RenderComponent<CreateUser>();

            cut.Instance.createUserModel = createUserDto;
            cut.Instance.availableRoles = new List<CreateUser.RoleSelection>
    {
        new() { Name = "Admin", IsSelected = true }
    };

            // Act
            await cut.InvokeAsync(() => cut.Instance.HandleCreateUser());

            // Assert
            cut.WaitForAssertion(() =>
            {
                var errorMessage = cut.Find("em").TextContent;
                Assert.Equal("Failed to create user: Invalid user data", errorMessage);
            }, timeout: TimeSpan.FromSeconds(1));
        }

    }
}
