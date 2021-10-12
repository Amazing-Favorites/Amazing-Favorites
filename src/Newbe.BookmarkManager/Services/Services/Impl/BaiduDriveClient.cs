using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Newbe.BookmarkManager.WebApi;
using Refit;
using WebExtensions.Net;
using WebExtensions.Net.Cookies;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;
using WebExtensions.Net.WebRequest;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveClient : IBaiduDriveClient
    {

        public const string Dir = "/apps/AmazingFavoritesZ/";
        public const string DataFileName = "af.json";
        public const string Path = Dir + DataFileName;
        private readonly IIdentityApi _identityApi;
        private readonly ILogger<BaiduDriveClient> _logger;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IBaiduApi _baiduApi;
        private static string? _authUrl = null;
        private readonly IBaiduPCSApi _baiduPcsApi;
        private readonly CryptoJS _cryptoJS;
        private readonly IWebExtensionsApi _webExtensionsApi;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClock _clock;
        private long? _fileId;

        public BaiduDriveClient(
            IIdentityApi identityApi,
            ILogger<BaiduDriveClient> logger,
            IBaiduApi baiduApi,
            CryptoJS cryptoJs,
            IClock clock,
            IBaiduPCSApi baiduPcsApi,
            IWebExtensionsApi webExtensionsApi,
            IHttpClientFactory httpClientFactory, IUserOptionsService userOptionsService)
        {
            _identityApi = identityApi;
            _logger = logger;
            _baiduApi = baiduApi;
            _cryptoJS = cryptoJs;
            _clock = clock;
            _baiduPcsApi = baiduPcsApi;
            _webExtensionsApi = webExtensionsApi;
            _httpClientFactory = httpClientFactory;
            _userOptionsService = userOptionsService;
        }

        public void LoadToken(string token)
        {
            BaiduApiAuthHeaderHandler.Token = token;
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
                    var scopes = "basic netdisk super_msg";
                    _authUrl = "https://openapi.baidu.com/oauth/2.0/authorize";
                    var clientId = "tPftmS1HNHNp6zUPXVdNR9frdQ2jNnoR";
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=token";
                    _authUrl += $"&redirect_uri={redirectUrl}";
                    //_authUrl += $"&redirect_uri=oob";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(",", scopes.Split(" ")))}";
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

        private async Task LoadTokenAsync(string token, string exp)
        {
            var options = await _userOptionsService.GetOptionsAsync();
            options.CloudBkFeature.AccessToken = token;
            if (Int64.TryParse(exp, out var time))
                options.CloudBkFeature.ExpireDate = DateTime.Now.ToLocalTime().AddSeconds(time);
            await _userOptionsService.SaveAsync(options);
        }
        public async Task<bool> TestAsync()
        {
            // await _webExtensionsApi.Cookies.Remove(new RemoveDetails()
            // {
            //     Name = "PANWEB",
            //     Url = "https://pan.baidu.com/"
            // });
            // var data = new Dictionary<string, object>
            // {
            //     {"path",  "/apps/AmazingFavoritesZ"},
            //     {"size", "1"},
            //     {"rtype", "3"},
            //     {"isdir", "1"},
            // };
            // var mergeResponse = await _baiduApi.CreateAsync(data);
            // if (mergeResponse?.Content != null && mergeResponse.)
            // {
            //     return true;
            // }
            return true;
        }
        public async Task<long?> UploadAsync(CloudBkCollection cloudBkCollection)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().ToArray();
            await using var stream = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().AsStream();
            var size = stream.Length;
            var md5Str = await _cryptoJS.Hex(bytes);
            var request = new Dictionary<string, object>
            {
                {"path", Path},
                {"size", size},
                {"rtype", "3"},
                {"isdir", "0"},
                {"autoinit", "1"},
                {"block_list", JsonSerializer.Serialize(new string[] {md5Str})}
            };
            var reCreateResponse = await _baiduApi.PreCreateAsync(request);
            _logger.LogInformation(reCreateResponse.Content?.Path);
            _logger.LogInformation(reCreateResponse.Content?.UploadId);
            if (reCreateResponse.IsSuccessStatusCode && reCreateResponse.Content != null)
            {
                var updateResponse = await _baiduPcsApi.UploadAsync(new UploadRequest()
                {
                    Path = Dir + DataFileName,
                    Uploadid = reCreateResponse.Content.UploadId,
                    Type = "tmpfile",
                    PartSeq = 0
                }, new StreamPart(stream, "af.json", "application/json"));
                _logger.LogInformation(updateResponse.Content?.UploadId);
                request = new Dictionary<string, object>
                {
                    {"path", Path},
                    {"size", size},
                    {"rtype", 3},
                    {"isdir", "0"},
                    {"uploadid", reCreateResponse.Content.UploadId},
                    {"block_list", JsonSerializer.Serialize(new string[] {md5Str})},
                };
                await _webExtensionsApi.Cookies.Remove(new RemoveDetails()
                {
                    Name = "PANWEB",
                    Url = "https://pan.baidu.com/"
                });
                var mergeResponse = await _baiduApi.CreateAsync(request);
                _logger.LogInformation("FSID:" + mergeResponse?.Content?.FsId.ToString());
                _fileId = mergeResponse.Content.FsId;
                return mergeResponse.Content.FsId;
            }


            return null;


        }

        public async Task<CloudBkCollection?> DownLoadFileByFileIdAsync()
        {
            if (!_fileId.HasValue)
                return null;
            var options = await _userOptionsService.GetOptionsAsync();
            var accessToken = options.CloudBkFeature.AccessToken;
            var dLinkResponse = await _baiduApi.GetFileMatesAsync(new BaiduFileMetasRequest()
            {
                FsIds = JsonSerializer.Serialize(new long[] { _fileId.Value }),
                DLink = 1
            });
            var dlink = dLinkResponse.Content.List.FirstOrDefault().DLink;
            dlink = dlink + "&access_token=" + accessToken;
            _logger.LogInformation("DLINK:" + dlink);
            var request = new HttpRequestMessage(HttpMethod.Get,
                dlink);
            request.Headers.Add("User-Agent", "pan.baidu.com");
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var re = await JsonSerializer.DeserializeAsync<CloudBkCollection>(responseStream);
                return re;
            }

            return null;
        }

        public async Task<long?> GetAfFieldId()
        {
            var response = await _baiduApi.SearchAsync(new BaiduSearchRequest()
            {
                Key = DataFileName,
                Dir = Dir,
            });

            if (response.IsSuccessStatusCode && response.Content != null && response.Content.Errno == 0)
            {
                _fileId = response.Content.List.FirstOrDefault().FsId;
                return _fileId;
            }

            return null;

        }
    }
}