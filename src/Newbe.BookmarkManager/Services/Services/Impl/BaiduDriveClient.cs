using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveClient:IBaiduDriveClient
    {
    
        public const string DataFileName = "af.data.json";
        private readonly IIdentityApi _identityApi;
        private readonly ILogger<BaiduDriveClient> _logger;
        
        private static string? _authUrl = null;

        public BaiduDriveClient(IIdentityApi identityApi, ILogger<BaiduDriveClient> logger)
        {
            _identityApi = identityApi;
            _logger = logger;
        }
    
        public Task<string?> LoginAsync(bool interactive)
        {
            async Task<string?> LoginCoreAsync()
            {
                //var options = _googleDriveOauthOptions.Value;
                var redirectUrl = await _identityApi.GetRedirectURL("");
                if (_authUrl == null)
                {
                    var scopes = "";
                    _authUrl = "https://openapi.baidu.com/oauth/2.0/authorize";
                    var clientId = "";
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=CODE";
                    _authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(" ", scopes))}";
                }
                var callbackUrl = await _identityApi.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
                {
                    Interactive = interactive,
                    Url = new HttpURL(_authUrl)
                });
                var token = callbackUrl.Split("#")[1].Split("&")[0].Split("=")[1];
                //LoadToken(token);
                _logger.LogInformation("Baidu Drive login success");
                return token;
            }
        }

        
        public Task<bool> TestAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}