﻿using Bunit;
using Xunit;
using BillingPocTwo.WebUI.Client.Layout;
using BillingPocTwo.WebUI.Client.Test.Helpers;
using BillingPocTwo.Shared.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;

namespace BillingPocTwo.WebUI.Client.Test
{
    public class NavMenuTests : TestContext
    {
        public NavMenuTests()
        {
            // Register the UserState service
            Services.AddScoped<UserState>();

            // Register a fake AuthenticationStateProvider
            Services.AddAuthorizationCore();
            Services.AddScoped<AuthenticationStateProvider, TestAuthenticationStateProvider>();
        }

        [Fact]
        public void NavMenu_ShouldRender()
        {
            // Arrange
            var cut = RenderComponent<NavMenu>();
            // Act
            var navMenu = cut.Find("nav");
            // Assert
            Assert.NotNull(navMenu);
        }
    }
}
