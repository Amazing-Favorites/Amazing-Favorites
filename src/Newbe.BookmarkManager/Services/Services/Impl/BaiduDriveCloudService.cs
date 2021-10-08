using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.SimpleData;
using Newbe.BookmarkManager.WebApi;
using Newtonsoft.Json;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveCloudService:ICloudService
    {
        private readonly IBaiduApi _baiduApi;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;
        private readonly IUserOptionsService _userOptionsService;
        private long? _fileId;
        public BaiduDriveCloudService(
            IBaiduApi baiduApi, 
            ISimpleDataStorage simpleDataStorage,
            IClock clock, 
            IUserOptionsService userOptionsService
            )
        {
            _baiduApi = baiduApi;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
            _userOptionsService = userOptionsService;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            // if (!await _baiduApi.TestAsync())
            // {
            //     return new CloudBkStatus(false, new GetCloudOutput());
            // }
            var fileDescription = await GetFileDescription();
            if (fileDescription == null)
            {
                return new CloudBkStatus(true, new GetCloudOutput()
                {
                    EtagVersion = 0,
                    LastUpdateTime = 0
                });
            }
            return new CloudBkStatus(true, new GetCloudOutput()
            {
                EtagVersion = 0,
                LastUpdateTime = 0
            });
            // var cloudBkCollection = await GetCloudDataAsync();
            // return new CloudBkStatus(etagVersion != fileDescription.EtagVersion, new GetCloudOutput
            // {
            //     EtagVersion = fileDescription.EtagVersion,
            //     LastUpdateTime = fileDescription.LastUpdateTime,
            //     CloudBkCollection = cloudBkCollection
            // });
        }

        private async Task<BaiduFile?> GetFileDescription()
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var response = await _baiduApi.SearchAsync(new BaiduSearchRequest()
            {
                AccessToken = options.CloudBkFeature.AccessToken,
                Key = Consts.Cloud.CloudDataFileName,
            });
            var fileList = JsonConvert.DeserializeObject<BaiduSearchResponse>(response?.Error?.Content)?.List;
            if (fileList == null)
            {
                return null;
            }

            var file = fileList.FirstOrDefault(a => a.ServerFileName.Contains(Consts.Cloud.CloudDataFileName));
            if (file != null)
            {
                _fileId = file.FsId;
            }
            return file;
        }
        private async Task<CloudBkCollection?> GetCloudDataAsync()
        {
            if (_fileId == null)
            {
                return default;
            }
            var options = await _userOptionsService.GetOptionsAsync();
            var dLinkResponse = await _baiduApi.GetFileMatesAsync(new BaiduFileMetasRequest()
            {
                AccessToken = options.CloudBkFeature.AccessToken,
                FsIds = "[" + _fileId + "]",
                DLink = 1

            });
            return null;
        }
        public Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            throw new System.NotImplementedException();
        }
    }
}