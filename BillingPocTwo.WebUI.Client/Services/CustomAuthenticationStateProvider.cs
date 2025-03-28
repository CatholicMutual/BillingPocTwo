using BillingPocTwo.WebUI.Client.Entities;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace BillingPocTwo.WebUI.Client.Services
{
    
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly UserState _userState;
        private readonly HttpClient _httpClient;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorage, UserState userState, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _userState = userState;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            var identity = string.IsNullOrEmpty(token)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");

            var user = new ClaimsPrincipal(identity);

            // Set the admin status based on the token claims
            _userState.IsAdmin = user.IsInRole("Admin");
            _userState.IsUser = user.IsInRole("User");
            _userState.Email = user.FindFirst(ClaimTypes.Email)?.Value;

            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            // Set the admin status based on the token claims
            _userState.IsAdmin = user.IsInRole("Admin");
            _userState.IsUser = user.IsInRole("User");
            _userState.Email = user.FindFirst(ClaimTypes.Email)?.Value;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            _userState.IsAdmin = false;
            _userState.IsUser = false;
            _userState.Email = null;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task LogoutAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await _httpClient.SendAsync(request);
            }

            await _localStorage.RemoveItemAsync("authToken");
            NotifyUserLogout();
        }
    }
}
