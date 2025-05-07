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
using Moq.Protected;
using System.Net;
using System.Net.Http;
using Blazored.SessionStorage;

namespace BillingPocTwo.WebUI.Client.Test
{
    //public class MainLayoutTests : TestContext
    //{
    //    private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
    //    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    //    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock; // Added this field

    //    public MainLayoutTests()
    //    {
    //        _jsRuntimeMock = new Mock<IJSRuntime>();
    //        _httpClientFactoryMock = new Mock<IHttpClientFactory>(); // Initialize the mock

    //        var userState = new UserState
    //        {
    //            IsAdmin = true,
    //            IsUser = true,
    //            Email = "user@example.com"
    //        };

    //        var sessionStorageMock = new Mock<ISessionStorageService>();
    //        var httpClient = CreateMockHttpClient();

    //        _httpClientFactoryMock
    //            .Setup(factory => factory.CreateClient("AuthApi"))
    //            .Returns(httpClient); // Setup the mock behavior

    //        var customAuthStateProvider = new CustomAuthenticationStateProvider(
    //            _httpClientFactoryMock.Object, // Use the initialized mock
    //            sessionStorageMock.Object,
    //            userState,
    //            httpClient
    //        );

    //        Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider); // Register the real instance
    //        Services.AddSingleton(_jsRuntimeMock.Object);
    //        Services.AddSingleton(userState);
    //        Services.AddAuthorizationCore();

    //        var fakeNavigationManager = new FakeNavigationManager(this);
    //        Services.AddSingleton<NavigationManager>(fakeNavigationManager);
    //    }

    //    //[Fact]
    //    //public void MainLayout_ShouldRenderCorrectly()
    //    //{
    //    //    // Arrange
    //    //    var cut = RenderComponent<MainLayout>();

    //    //    // Act
    //    //    var image = cut.Find("a.external-link img");

    //    //    // Assert
    //    //    Assert.Equal("Catholic Mutual Group", image.GetAttribute("alt"));
    //    //}

    //    //[Fact]
    //    //public async Task MainLayout_ShouldInvokeLogoutOnClose()
    //    //{
    //    //    // Arrange
    //    //    var httpClient = CreateMockHttpClient();

    //    //    _httpClientFactoryMock
    //    //        .Setup(factory => factory.CreateClient("AuthApi"))
    //    //        .Returns(httpClient); // Now mock the named client for "AuthApi"

    //    //    var cut = RenderComponent<MainLayout>();

    //    //    //// Act
    //    //    //await cut.InvokeAsync(() => cut.Instance.LogoutOnClose());

    //    //    // Assert
    //    //    // No need to verify a mock; the real instance is used
    //    //}

    //    //[Fact]
    //    //public async Task MainLayout_ShouldNavigateToHomeOnLogout()
    //    //{
    //    //    // Arrange
    //    //    var userState = new UserState
    //    //    {
    //    //        IsAdmin = true,
    //    //        IsUser = true,
    //    //        Email = "user@example.com"
    //    //    };

    //    //    var sessionStorageMock = new Mock<ISessionStorageService>();
    //    //    var httpClient = CreateMockHttpClient();

    //    //    var customAuthStateProvider = new CustomAuthenticationStateProvider(
    //    //        _httpClientFactoryMock.Object, // Use the initialized mock
    //    //        sessionStorageMock.Object,
    //    //        userState,
    //    //        httpClient
    //    //    );

    //    //    _httpClientFactoryMock
    //    //        .Setup(factory => factory.CreateClient("AuthApi"))
    //    //        .Returns(httpClient); // Now mock the named client for "AuthApi"

    //    //    // Override any previously registered AuthenticationStateProvider
    //    //    Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);

    //    //    var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
    //    //    var cut = RenderComponent<MainLayout>();

    //    //    // Act
    //    //    await cut.InvokeAsync(() => cut.Instance.Logout());

    //    //    // Assert
    //    //    Assert.Equal("http://localhost/", navigationManager.Uri);
    //    //}

    //    //[Fact]
    //    //public async Task MainLayout_ShouldImportLogoutScriptOnFirstRender()
    //    //{
    //    //    // Arrange
    //    //    var cut = RenderComponent<MainLayout>();

    //    //    // Act
    //    //    await cut.InvokeAsync(() => cut.Instance.OnAfterRenderAsync(true));

    //    //    // Assert
    //    //    _jsRuntimeMock.Verify(
    //    //        x => x.InvokeAsync<object>(
    //    //            "import",
    //    //            It.Is<object[]>(args => args.Length == 1 && args[0]!.ToString() == "./js/logout.js")),
    //    //        Times.Once
    //    //    );
    //    //}

    //    private static HttpClient CreateMockHttpClient()
    //    {
    //        var handlerMock = new Mock<HttpMessageHandler>();

    //        handlerMock
    //            .Protected()
    //            .Setup<Task<HttpResponseMessage>>(
    //                "SendAsync",
    //                ItExpr.IsAny<HttpRequestMessage>(),
    //                ItExpr.IsAny<CancellationToken>()
    //            )
    //            .ReturnsAsync(new HttpResponseMessage
    //            {
    //                StatusCode = HttpStatusCode.OK,
    //                Content = new StringContent("{}") // simulate a successful response
    //            });

    //        return new HttpClient(handlerMock.Object)
    //        {
    //            BaseAddress = new Uri("https://localhost:7192/")
    //        };
    //    }
    //}
}
