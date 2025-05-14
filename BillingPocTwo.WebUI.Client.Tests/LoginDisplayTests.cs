using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Xunit;
using BillingPocTwo.WebUI.Client.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;

namespace BillingPocTwo.WebUI.Client.Tests
{
    public class LoginDisplayTests : TestContext
    {
        [Fact]
        public void ShowsLogin_WhenNotAuthenticated()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: false));
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
            Services.AddScoped<Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager>(
                sp => new Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager(
                    sp.GetRequiredService<IJSRuntime>()));

            var cut = RenderComponent<LoginDisplay>();

            // Assert: Login button is present, Logout is not
            Assert.Contains("Login", cut.Markup);
            Assert.DoesNotContain("Logout", cut.Markup);
        }

        [Fact]
        public void ShowsLogout_WhenAuthenticated()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: true));

            // Register mock IJSRuntime and SignOutSessionStateManager
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
            Services.AddScoped<Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager>(
                sp => new Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager(
                    sp.GetRequiredService<IJSRuntime>()));

            var cut = RenderComponent<LoginDisplay>();
            cut.Markup.Contains("Log out");
        }

        [Fact]
        public void ShowsLogoutAndUserName_WhenAuthenticated()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: true));
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
            Services.AddScoped<Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager>(
                sp => new Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager(
                    sp.GetRequiredService<IJSRuntime>()));

            var cut = RenderComponent<LoginDisplay>();

            // Assert: Logout button and user name are present, Login is not
            Assert.Contains("Logout", cut.Markup);
            Assert.Contains("Test User", cut.Markup);
            Assert.DoesNotContain("Login", cut.Markup);
        }

        [Fact]
        public void ClickingLogin_NavigatesToLoginPage()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: false));
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);
            Services.AddScoped<Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager>(
                sp => new Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager(
                    sp.GetRequiredService<IJSRuntime>()));

            // Use bUnit's FakeNavigationManager
            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<LoginDisplay>();

            // Find and click the Login button
            cut.Find("button").Click();

            // Assert: Navigated to the login page
            Assert.Contains("authentication/login", navMan.Uri);
        }

        [Fact]
        public void ClickingLogout_CallsSignOutManager_AndNavigatesToLogout()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: true));
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            // Mock SignOutSessionStateManager
            var signOutManagerMock = new Mock<Microsoft.AspNetCore.Components.WebAssembly.Authentication.SignOutSessionStateManager>(jsRuntimeMock.Object);
            Services.AddScoped(_ => signOutManagerMock.Object);

            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<LoginDisplay>();

            // Find and click the Logout button
            cut.Find("button").Click();

            // Assert: SignOutManager.SetSignOutState was called
            signOutManagerMock.Verify(m => m.SetSignOutState(), Times.Once);

            // Assert: Navigated to the logout page
            Assert.Contains("authentication/logout", navMan.Uri);
        }

    }

    // Helper for faking auth state
    public class TestAuthStateProvider : AuthenticationStateProvider
    {
        private readonly bool _isAuthenticated;
        private readonly string[] _roles;

        public TestAuthStateProvider(bool isAuthenticated, string[]? roles = null)
        {
            _isAuthenticated = isAuthenticated;
            _roles = roles ?? Array.Empty<string>();
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new List<System.Security.Claims.Claim>();
            if (_isAuthenticated)
            {
                claims.Add(new System.Security.Claims.Claim("name", "Test User"));
                if (_roles.Length > 0)
                {
                    // Use the same claim type as your NavMenu expects
                    claims.Add(new System.Security.Claims.Claim("roles", System.Text.Json.JsonSerializer.Serialize(_roles)));
                }
            }
            var identity = _isAuthenticated
                ? new System.Security.Claims.ClaimsIdentity(claims, "Test")
                : new System.Security.Claims.ClaimsIdentity();
            var user = new System.Security.Claims.ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }

}
