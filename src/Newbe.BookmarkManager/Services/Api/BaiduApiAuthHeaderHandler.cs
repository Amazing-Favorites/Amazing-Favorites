using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduApiAuthHeaderHandler : DelegatingHandler
    {
        public static string? Token = null!;
        public BaiduApiAuthHeaderHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestString = request.RequestUri.ToString();
            requestString += $"&access_token=" + Token;
            request.RequestUri = new Uri(requestString);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

    }

}