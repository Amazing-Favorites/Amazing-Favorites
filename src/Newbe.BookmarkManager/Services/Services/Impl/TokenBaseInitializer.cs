using System.Net.Http.Headers;
using Google.Apis.Http;

namespace Newbe.BookmarkManager.Services
{
    public class TokenBaseInitializer : IConfigurableHttpClientInitializer
    {
        private readonly string _token;

        public TokenBaseInitializer(string token)
        {
            _token = token;
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}