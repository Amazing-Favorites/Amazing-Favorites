
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Authentication.WebAssembly.Msal.Models;
using Microsoft.Graph;
using System.Net.Http.Headers;
using WebExtensions.Net.Identity;

namespace Newbe.BookmarkManager;
public static class GraphClientExtensions
{
    public static IServiceCollection AddGraphClient(
        this IServiceCollection services, IConfiguration configuration, params string[] scopes)
    {
        services.AddMsalAuthentication(
            options =>
            {
                configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                foreach (var scope in scopes)
                {
                    options.ProviderOptions.AdditionalScopesToConsent.Add(scope);
                }
                //options.ProviderOptions.Authentication.RedirectUri = "https://cchkdfakpmchaoodoodcmjgiojcknhnc.chromiumapp.org";
            });
        services.Configure<RemoteAuthenticationOptions<MsalProviderOptions>>(
            options =>
            {
                foreach (var scope in scopes)
                {
                    options.ProviderOptions.AdditionalScopesToConsent.Add(scope);
                }
            });

        services.AddScoped<IAuthenticationProvider,
            NoOpGraphAuthenticationProvider>();
        services.AddScoped<IHttpProvider, HttpClientHttpProvider>(sp =>
            new HttpClientHttpProvider(new HttpClient()));
        services.AddScoped(sp =>
        {
            return new GraphServiceClient(
                sp.GetRequiredService<IAuthenticationProvider>(),
                sp.GetRequiredService<IHttpProvider>());
        });

        return services;
    }

    private class NoOpGraphAuthenticationProvider : IAuthenticationProvider
    {
        public NoOpGraphAuthenticationProvider(IAccessTokenProviderAccessor tokenProvider)
        {
            TokenProvider = tokenProvider;
        }

        public IAccessTokenProviderAccessor TokenProvider { get; }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var result = await TokenProvider.TokenProvider.RequestAccessToken(
                new AccessTokenRequestOptions()
                {
                    Scopes = new[] {
                        "https://graph.microsoft.com/User.Read",
                        "https://graph.microsoft.com/Files.Read",
                        "https://graph.microsoft.com/Files.Read.All",
                        "https://graph.microsoft.com/Files.Read.Selected",
                        "https://graph.microsoft.com/Files.ReadWrite",
                        "https://graph.microsoft.com/Files.ReadWrite.All",
                        "https://graph.microsoft.com/Files.ReadWrite.AppFolder",
                        "https://graph.microsoft.com/Files.ReadWrite.Selected",
                    },
                });

            if (result.TryGetToken(out var token))
            {
                request.Headers.Authorization ??= new AuthenticationHeaderValue(
                    "Bearer", token.Value);
            }
        }
    }

    private class HttpClientHttpProvider : IHttpProvider
    {
        private readonly HttpClient http;

        public HttpClientHttpProvider(HttpClient http)
        {
            this.http = http;
        }

        public ISerializer Serializer { get; } = new Serializer();

        public TimeSpan OverallTimeout { get; set; } = TimeSpan.FromSeconds(300);

        public void Dispose()
        {
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return http.SendAsync(request);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return http.SendAsync(request, completionOption, cancellationToken);
        }
    }
}
