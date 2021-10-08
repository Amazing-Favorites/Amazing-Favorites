using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;
using WebExtensions.Net.WebRequest;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveClient:IBaiduDriveClient
    {
    
        public const string DataFileName = "af.data.json";
        private readonly IIdentityApi _identityApi;
        private readonly ILogger<BaiduDriveClient> _logger;
        private readonly IBaiduApi _baiduApi;
        private readonly IUserOptionsService _userOptionsService;
        private static string? _authUrl = null;
        
        private static string? _tokenUrl = null;

        public BaiduDriveClient(IIdentityApi identityApi,
            ILogger<BaiduDriveClient> logger,
            IBaiduApi baiduApi,
            IUserOptionsService userOptionsService
            )
        {
            _identityApi = identityApi;
            _logger = logger;
            _baiduApi = baiduApi;
            _userOptionsService = userOptionsService;
        }
    
        public async Task<string?> LoginAsync(bool interactive)
        {

            try
            {
                return await LoginCoreAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to login Baidu Drive");
                if (interactive)
                {
                    throw;
                }
            }
            return default;
            async Task<string?> LoginCoreAsync()
            {
                var redirectUrl = await _identityApi.GetRedirectURL("");
                _logger.LogInformation($"{redirectUrl}");
                if (_authUrl == null)
                {
                    var scopes = "basic netdisk";
                    _authUrl = "https://openapi.baidu.com/oauth/2.0/authorize";
                    var clientId = "";
                    var key = "";
                    var secret = "";
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=token";
                    _authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
                    //_authUrl += $"&redirect_uri=oob";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(" ", scopes))}";
                    _authUrl += $"&state=STATE";
                }
                _logger.LogInformation($"{_authUrl}");
                var callbackUrl = await _identityApi.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
                {
                    Interactive = interactive,
                    Url = new HttpURL(_authUrl)
                });
                var token = callbackUrl.Split('#')[1].Split('&')[1].Split('=')[1];
                var exp = callbackUrl.Split('#')[1].Split('&')[0].Split('=')[1];
                _logger.LogInformation($"callbackUrl:{callbackUrl}");
                _logger.LogInformation($"exp:{exp}");
                await LoadTokenAsync(token, exp);
                return token;
            }
        }
        
        public async Task LoadTokenAsync(string token,string exp)
        {
            var options = await _userOptionsService.GetOptionsAsync();
            options.CloudBkFeature.AccessToken = token;
            if (Int64.TryParse(exp, out var time))
                options.CloudBkFeature.ExpireDate = DateTime.Now.AddSeconds(time);
            await _userOptionsService.SaveAsync(options);
        }
        public Task<bool> TestAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}