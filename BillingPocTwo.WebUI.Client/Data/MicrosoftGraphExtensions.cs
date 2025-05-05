using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace BillingPocTwo.WebUI.Client.Data
{
    public static class MicrosoftGraphExtensions
    {
        public static IServiceCollection AddMicrosoftGraphClient(this IServiceCollection services, params string[] scopes)
        {
            services.AddScoped<IAuthenticationProvider>(sp =>
            {
                var accessTokenProvider = sp.GetRequiredService<Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider>();
                return new GraphAuthenticationProvider(accessTokenProvider, scopes);
            });

            services.AddScoped<GraphServiceClient>(sp =>
            {
                var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
                return new GraphServiceClient(authProvider);
            });

            return services;
        }
    }
}
