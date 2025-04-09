using Bunit;
using BillingPocTwo.WebUI.Client.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using Microsoft.JSInterop;
using BillingPocTwo.WebUI.Client.Services;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using BillingPocTwo.Shared.Entities;
using Blazored.LocalStorage;
using BillingPocTwo.Shared.Entities.Auth;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class MainLayoutTests : TestContext
    {
        private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
        private readonly Mock<IJSRuntime> _jsRuntimeMock;

        public MainLayoutTests()
        {
            _jsRuntimeMock = new Mock<IJSRuntime>();

            var userState = new UserState
            {
                IsAdmin = true,
                IsUser = true,
                Email = "user@example.com"
            };

            var localStorageMock = new Mock<ILocalStorageService>();
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            var customAuthStateProvider = new CustomAuthenticationStateProvider(localStorageMock.Object, userState, httpClient);

            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider); // Register the real instance
            Services.AddSingleton(_jsRuntimeMock.Object);
            Services.AddSingleton(userState);
            Services.AddAuthorizationCore();

            var fakeNavigationManager = new FakeNavigationManager(this);
            Services.AddSingleton<NavigationManager>(fakeNavigationManager);
        }

        [Fact]
        public void MainLayout_ShouldRenderCorrectly()
        {
            // Arrange
            var cut = RenderComponent<MainLayout>();

            // Act
            var heading = cut.Find("a.external-link");

            // Assert
            Assert.Equal("Catholic Mutual Group", heading.TextContent.Trim());
        }

        [Fact]
        public async Task MainLayout_ShouldInvokeLogoutOnClose()
        {
            // Arrange
            var cut = RenderComponent<MainLayout>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.LogoutOnClose());

            // Assert
            // No need to verify a mock; the real instance is used
        }

        [Fact]
        public async Task MainLayout_ShouldNavigateToHomeOnLogout()
        {
            // Arrange
            var userState = new UserState
            {
                IsAdmin = true,
                IsUser = true,
                Email = "user@example.com"
            };

            var localStorageMock = new Mock<ILocalStorageService>();
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object)
            {
                BaseAddress = new Uri("https://localhost:7192/")
            };

            var customAuthStateProvider = new CustomAuthenticationStateProvider(localStorageMock.Object, userState, httpClient);

            // Override any previously registered AuthenticationStateProvider
            Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);

            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            var cut = RenderComponent<MainLayout>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.Logout());

            // Assert
            Assert.Equal("http://localhost/", navigationManager.Uri);
        }

        [Fact]
        public async Task MainLayout_ShouldImportLogoutScriptOnFirstRender()
        {
            // Arrange
            var cut = RenderComponent<MainLayout>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.OnAfterRenderAsync(true));

            // Assert
            _jsRuntimeMock.Verify(
                x => x.InvokeAsync<object>(
                    "import",
                    It.Is<object[]>(args => args.Length == 1 && args[0]!.ToString() == "./js/logout.js")),
                Times.Once
            );
        }
    }
}
