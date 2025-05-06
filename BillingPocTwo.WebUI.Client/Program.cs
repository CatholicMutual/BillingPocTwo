using BillingPocTwo.WebUI.Client;
using BillingPocTwo.WebUI.Client.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using BillingPocTwo.WebUI.Client.Services;
using BillingPocTwo.Shared.Entities.Auth;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register HttpClient for APIs
builder.Services.AddHttpClient("AuthApi", client => client.BaseAddress = new Uri("https://localhost:7192/"));
builder.Services.AddHttpClient("BillingDataApi", client => client.BaseAddress = new Uri("https://localhost:7251/"));
builder.Services.AddHttpClient("BillingPocTwo.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddMicrosoftGraphClient("https://graph.microsoft.com/User.Read");

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    // Use either .default or resource-specific scopes, but not both
    options.ProviderOptions.DefaultAccessTokenScopes.Clear();
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("profile");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
    options.ProviderOptions.Authentication.PostLogoutRedirectUri = "/";
    options.UserOptions.RoleClaim = "roles";
});

// Add Blazored LocalStorage
builder.Services.AddBlazoredSessionStorage();

// Register UserState
builder.Services.AddScoped<UserState>();

await builder.Build().RunAsync();
