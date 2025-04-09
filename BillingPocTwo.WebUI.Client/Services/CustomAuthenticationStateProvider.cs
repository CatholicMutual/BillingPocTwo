using BillingPocTwo.Shared.Entities.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace BillingPocTwo.WebUI.Client.Services
{
    
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory _httpClientFactory; // Added IHttpClientFactory
        private readonly ILocalStorageService _localStorage;
        private readonly UserState _userState;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, UserState userState, HttpClient httpClient)
        {
            _httpClientFactory = httpClientFactory; 
            _localStorage = localStorage;
            _userState = userState;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = JwtParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            _currentUser = new ClaimsPrincipal(identity);

            return new AuthenticationState(_currentUser);
        }

        public void NotifyUserAuthentication(string token)
        {
            var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

            _userState.IsAdmin = false;
            _userState.IsUser = false;
            _userState.Email = null;
            _userState.FirstName = null;
            _userState.LastName = null;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        public async Task LogoutAsync()
        {
            var client = _httpClientFactory.CreateClient("AuthApi"); // Use the named HttpClient
            await client.PostAsync("api/auth/logout", null); // Relative URI works because BaseAddress is set

            // Clear local storage and notify state change
            await _localStorage.RemoveItemAsync("authToken");
            NotifyUserLogout();
        }
    }
}
