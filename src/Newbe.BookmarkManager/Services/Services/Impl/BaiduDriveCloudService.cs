using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using Newbe.BookmarkManager.Services.SimpleData;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public class BaiduDriveCloudService : ICloudService
    {
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IBaiduDriveClient _baiduDriveClient;
        public BaiduDriveCloudService(
            ISimpleDataStorage simpleDataStorage,
            IClock clock,
            IUserOptionsService userOptionsService,
            IBaiduDriveClient baiduDriveClient
            )
        {
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
            _userOptionsService = userOptionsService;
            _baiduDriveClient = baiduDriveClient;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            if (!await _baiduDriveClient.TestAsync())
            {
                return new CloudBkStatus(false, new GetCloudOutput());
            }
            var cloudBkCollection = await _baiduDriveClient.DownLoadFileByFileIdAsync();
            if (cloudBkCollection == null)
            {
                return new CloudBkStatus(true, new GetCloudOutput()
                {
                    EtagVersion = 0,
                    LastUpdateTime = 0,
                });
            }
            return new CloudBkStatus(etagVersion != cloudBkCollection.EtagVersion, new GetCloudOutput
            {
                LastUpdateTime = cloudBkCollection.LastUpdateTime,
                EtagVersion = cloudBkCollection.EtagVersion,
                CloudBkCollection = cloudBkCollection
            });
        }
        private long MD5ToLong(string md5)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(md5);
            var re = BitConverter.ToInt64(bytes);
            return re;
        }
        public async Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            if (!await _baiduDriveClient.TestAsync())
            {
                return new SaveToCloudOutput
                {
                    IsOk = false,
                    Message = "baidu not login"
                };
            }
            await _baiduDriveClient.UploadAsync(cloudBkCollection);
            var baiduDriveStatics = await _simpleDataStorage.GetOrDefaultAsync<GoogleDriveStatics>();
            baiduDriveStatics.LastSuccessUploadTime = _clock.UtcNow;
            await _simpleDataStorage.SaveAsync(baiduDriveStatics);
            var re = new SaveToCloudOutput
            {
                IsOk = true
            };
            return re;
        }

    }
}