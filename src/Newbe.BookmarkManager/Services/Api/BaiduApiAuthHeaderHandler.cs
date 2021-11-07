using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduApiAuthHeaderHandler : DelegatingHandler
    {
        public static string? Token = null!;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestString = request.RequestUri.ToString();
            requestString += $"&access_token=" + Token;
            request.RequestUri = new Uri(requestString);
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("pan.baidu.com", ""));
            request.Headers.Add("mode", "no-cors");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}