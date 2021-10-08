using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.SimpleData;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveCloudService:ICloudService
    {
        private readonly IBaiduApi _baiduApi;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IHttpClientFactory _httpClientFactory;
        private long? _fileId;
        
        public BaiduDriveCloudService(
            IBaiduApi baiduApi, 
            ISimpleDataStorage simpleDataStorage,
            IClock clock, 
            IUserOptionsService userOptionsService, IHttpClientFactory httpClientFactory)
        {
            _baiduApi = baiduApi;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
            _userOptionsService = userOptionsService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            var fileDescription = await GetFileDescription();
            if (fileDescription == null)
            {
                return new CloudBkStatus(true, new GetCloudOutput()
                {
                    EtagVersion = 0,
                    LastUpdateTime = 0,
                });
            }
            var cloudBkCollection = await GetCloudDataAsync();
             return new CloudBkStatus(etagVersion != fileDescription.EtagVersion, new GetCloudOutput
             {
                 LastUpdateTime = fileDescription.LastUpdateTime,
                 EtagVersion = fileDescription.EtagVersion,
                 CloudBkCollection = cloudBkCollection
             });
        }

        private async Task<CloudDataDescription?> GetFileDescription()
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var accessToken = options.CloudBkFeature.AccessToken;
            var response = await _baiduApi.SearchAsync(new BaiduSearchRequest()
            {
                AccessToken = accessToken,
                Key = Consts.Cloud.CloudDataFileName,
            });
            if (response.Content == null)
            {
                return null;
            }
            var file = response.Content.List.FirstOrDefault(a => a.ServerFileName.Contains(Consts.Cloud.CloudDataFileName));
            if (file != null)
            {
                _fileId = file.FsId;
            }
            var re = new CloudDataDescription
            {
                LastUpdateTime = file.ServerMTime,
                EtagVersion = MD5ToLong(file.MD5)
            };
            
            return re;
        }
        private async Task<CloudBkCollection?> GetCloudDataAsync()
        {
            if (_fileId == null)
            {
                return default;
            }
            var options = await _userOptionsService.GetOptionsAsync();
            var accessToken = options.CloudBkFeature.AccessToken;
            var dLinkResponse = await _baiduApi.GetFileMatesAsync(new BaiduFileMetasRequest()
            {
                AccessToken = accessToken,
                FsIds = "[" + _fileId + "]",
                DLink = 1
            });
            var dlink = dLinkResponse.Content.List.FirstOrDefault().DLink;
            return await GetDLinkFileAsync(dlink);
        }


        private async Task<CloudBkCollection?> GetDLinkFileAsync(string dlink)
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var accessToken = options.CloudBkFeature.AccessToken;
            dlink = dlink + "&access_token=" + accessToken;
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

        private long MD5ToLong(string md5)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(md5);  
            var re = BitConverter.ToInt64(bytes);
            return re;
        }
        public Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            throw new System.NotImplementedException();
        }
    }
}