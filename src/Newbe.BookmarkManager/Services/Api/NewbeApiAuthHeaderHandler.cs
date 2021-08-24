using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    internal class NewbeApiAuthHeaderHandler : DelegatingHandler
    {
        private readonly IUserOptionsService _userOptionsService;

        public NewbeApiAuthHeaderHandler(
            IUserOptionsService userOptionsService)
        {
            _userOptionsService = userOptionsService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var token = options?.PinyinFeature?.AccessToken;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", $"id_token=\"{token}\"");
            }
            //potentially refresh token here if it has expired etc.
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}