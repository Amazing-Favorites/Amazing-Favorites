using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Newbe.BookmarkManager
{
    public class StaticAuthProvider : IAuthenticationProvider
    {
        public static string? Token = null!;

        // Function called every time the GraphServiceClient makes a call
        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Add the token to the Authorization header
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);
            return Task.CompletedTask;
        }
    }

    public class HttpClientHttpProvider : IHttpProvider
    {
        private readonly HttpClient _http;

        public HttpClientHttpProvider(HttpClient http)
        {
            this._http = http;
        }

        public ISerializer Serializer { get; } = new Serializer();

        public TimeSpan OverallTimeout { get; set; } = TimeSpan.FromSeconds(300);

        public void Dispose()
        {
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _http.SendAsync(request);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return _http.SendAsync(request, completionOption, cancellationToken);
        }
    }
}