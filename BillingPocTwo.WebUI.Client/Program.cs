using BillingPocTwo.WebUI.Client;
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
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for APIs
builder.Services.AddHttpClient("AuthApi", client => client.BaseAddress = new Uri("https://localhost:7192/"));
builder.Services.AddHttpClient("BillingDataApi", client => client.BaseAddress = new Uri("https://localhost:7251/"));
builder.Services.AddHttpClient("BillingPocTwo.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Add Blazored LocalStorage
builder.Services.AddBlazoredSessionStorage();

// Add authentication and authorization
builder.Services.AddApiAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Register UserState
builder.Services.AddScoped<UserState>();

await builder.Build().RunAsync();
