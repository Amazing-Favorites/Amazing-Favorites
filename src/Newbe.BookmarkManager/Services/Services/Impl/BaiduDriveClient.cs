using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Newbe.BookmarkManager.WebApi;
using Refit;
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
        private readonly IBaiduPCSApi _baiduPcsApi;
        private readonly CryptoJS _cryptoJS;
        private static string? _tokenUrl = null;
        private readonly IClock _clock;

        public BaiduDriveClient(IIdentityApi identityApi,
            ILogger<BaiduDriveClient> logger,
            IBaiduApi baiduApi,
            IUserOptionsService userOptionsService, CryptoJS cryptoJs, IClock clock, IBaiduPCSApi baiduPcsApi)
        {
            _identityApi = identityApi;
            _logger = logger;
            _baiduApi = baiduApi;
            _userOptionsService = userOptionsService;
            _cryptoJS = cryptoJs;
            _clock = clock;
            _baiduPcsApi = baiduPcsApi;
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
                    var clientId = "tPftmS1HNHNp6zUPXVdNR9frdQ2jNnoR";
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=token";
                    //_authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
                    _authUrl += $"&redirect_uri={redirectUrl}";
                    //_authUrl += $"&redirect_uri=oob";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(",", scopes))}";
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
        public async Task UploadAsync(CloudBkCollection cloudBkCollection)
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var accessToken = options.CloudBkFeature.AccessToken;
            var bytes = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().ToArray();
            await using var stream = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().AsStream();
            var md5Str = await _cryptoJS.Hex(bytes);
            var data = new Dictionary<string, object>
            {
                {"path", "/test/af.json"},
                {"size", "4096"},
                {"rtype", "3"},
                {"isdir", "0"},
                {"autoinit", "1"},
                {"block_list", JsonSerializer.Serialize(new string[] {md5Str})}
            };
            var reCreateResponse = await _baiduApi.PreCreateAsync2(new BaiduRequest()
            {
                AccessToken = accessToken
            }, data);
            _logger.LogInformation(reCreateResponse.Content?.Path);
            _logger.LogInformation(reCreateResponse.Content?.UploadId);
            if (reCreateResponse.IsSuccessStatusCode&& reCreateResponse.Content !=null)
            {
                var updateRequest = await _baiduPcsApi.UploadAsync(new UploadRequest()
                {
                    AccessToken = accessToken,
                    Path = "/test/af.json",
                    Uploadid = reCreateResponse.Content.UploadId,
                    Type = "tmpfile",
                    PartSeq = 0
                },new StreamPart(stream,"af.json","application/json"));
                
                data = new Dictionary<string, object>
                {
                    {"path", "/test/af.json"},
                    {"size", "4096"},
                    {"rtype", "3"},
                    {"isdir", "0"},
                    {"uploadid", reCreateResponse.Content.UploadId},
                    {"block_list", JsonSerializer.Serialize(new string[] {md5Str})}
                };
                var mergeResponse = await _baiduApi.CreateAsync(new BaiduRequest()
                {
                    AccessToken = accessToken
                }, data);
            }

            
            


        }
    }
}