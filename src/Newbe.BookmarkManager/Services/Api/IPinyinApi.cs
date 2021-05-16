using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IPinyinApi
    {
        [Post("/pinyin")]
        Task<ApiResponse<PinyinOutput>> GetPinyinAsync(PinyinInput input);
    }
    
    internal class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IUserOptionsRepository _userOptionsRepository;

        public AuthHeaderHandler(
            IUserOptionsRepository userOptionsRepository)
        {
            _userOptionsRepository = userOptionsRepository;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var options = await _userOptionsRepository.GetOptionsAsync();
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