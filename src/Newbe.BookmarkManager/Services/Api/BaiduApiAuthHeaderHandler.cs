using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduApiAuthHeaderHandler : DelegatingHandler
    {
        public BaiduApiAuthHeaderHandler()
        {

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Remove("Cookie");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

    }

}