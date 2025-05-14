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
    public class NavMenuTests : TestContext
    {
        [Fact]
        public void NavMenu_NotVisible_WhenNotAuthenticated()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: false));

            // Register mock IJSRuntime if NavMenu or its children require it
            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var cut = RenderComponent<NavMenu>();

            // Find the nav element
            var nav = cut.Find("nav");
            // Assert: The nav should not contain any links
            Assert.Empty(nav.QuerySelectorAll("a"));
        }

        [Fact]
        public void NavMenu_ShowsMenuItems_WhenAuthenticatedWithRoles()
        {
            Services.AddAuthorizationCore();
            // Provide a user with a role claim
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(
                isAuthenticated: true,
                roles: new[] { "BillingUser" }
            ));

            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var cut = RenderComponent<NavMenu>();

            // Assert: Should see a menu item only visible to authenticated users with roles
            Assert.Contains("Accounts Register", cut.Markup);
            Assert.Contains("Client Search", cut.Markup);
            Assert.Contains("User Profile", cut.Markup);
        }

        [Fact]
        public void NavMenu_ShowsNoRolesMessage_WhenAuthenticatedWithoutRoles()
        {
            Services.AddAuthorizationCore();
            // Authenticated but no roles
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(
                isAuthenticated: true,
                roles: Array.Empty<string>()
            ));

            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var cut = RenderComponent<NavMenu>();

            Assert.Contains("You do not have the required roles to access the menu.", cut.Markup);
        }

        [Fact]
        public void ClickingAccountsRegister_NavigatesToAccountsRegister()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(
                isAuthenticated: true,
                roles: new[] { "BillingUser" }
            ));

            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<NavMenu>();

            // Find the "Accounts Register" link and click it
            var link = cut.FindAll("a").FirstOrDefault(a => a.TextContent.Contains("Accounts Register"));
            Assert.NotNull(link);

            var href = link.GetAttribute("href");
            navMan.NavigateTo(href!);

            Assert.Contains("/accounts-register", navMan.Uri, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClickingClientSearch_NavigatesToSearch()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(
                isAuthenticated: true,
                roles: new[] { "BillingUser" }
            ));

            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<NavMenu>();

            // Find the "Client Search" link and click it
            var link = cut.FindAll("a").FirstOrDefault(a => a.TextContent.Contains("Client Search"));
            Assert.NotNull(link);

            var href = link.GetAttribute("href");
            navMan.NavigateTo(href!);

            Assert.Contains("search", navMan.Uri, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ClickingUserProfile_NavigatesToUserProfile()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(
                isAuthenticated: true,
                roles: new[] { "BillingUser" }
            ));

            var jsRuntimeMock = new Mock<IJSRuntime>();
            Services.AddSingleton<IJSRuntime>(jsRuntimeMock.Object);

            var navMan = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

            var cut = RenderComponent<NavMenu>();

            // Find the "User Profile" link and click it
            var link = cut.FindAll("a").FirstOrDefault(a => a.TextContent.Contains("User Profile"));
            Assert.NotNull(link);

            var href = link.GetAttribute("href");
            navMan.NavigateTo(href!);

            Assert.Contains("user-profile", navMan.Uri, StringComparison.OrdinalIgnoreCase);
        }
    }
}
