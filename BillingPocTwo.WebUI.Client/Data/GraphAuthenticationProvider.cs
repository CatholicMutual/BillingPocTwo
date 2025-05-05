using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BillingPocTwo.WebUI.Client.Data
{
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        private readonly Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider _provider;
        private readonly string[] _scopes;

        public GraphAuthenticationProvider(Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider provider, string[] scopes)
        {
            _provider = provider;
            _scopes = scopes;
        }

        public async Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _provider.RequestAccessToken(new AccessTokenRequestOptions
            {
                Scopes = _scopes
            });

            if (result.TryGetToken(out var token))
            {
                request.Headers.Add("Authorization", new[] { $"Bearer {token.Value}" });
            }
        }
    }
}
