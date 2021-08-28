using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.SimpleData;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public class OneDriveCloudService : ICloudService
    {
        private readonly IOneDriveClient _oneDriveClient;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;

        public OneDriveCloudService(
            IOneDriveClient oneDriveClient,
            ISimpleDataStorage simpleDataStorage,
            IClock clock)
        {
            _oneDriveClient = oneDriveClient;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            if (!await _oneDriveClient.TestAsync())
            {
                return new CloudBkStatus(false, new GetCloudOutput());
            }

            var fileDescription = await _oneDriveClient.GetFileDescriptionAsync();
            if (fileDescription == null)
            {
                return new CloudBkStatus(true, new GetCloudOutput()
                {
                    EtagVersion = 0,
                    LastUpdateTime = 0
                });
            }

            var cloudBkCollection = await _oneDriveClient.GetCloudDataAsync();
            return new CloudBkStatus(etagVersion != fileDescription.EtagVersion, new GetCloudOutput
            {
                EtagVersion = fileDescription.EtagVersion,
                LastUpdateTime = fileDescription.LastUpdateTime,
                CloudBkCollection = cloudBkCollection
            });
        }

        public async Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            if (!await _oneDriveClient.TestAsync())
            {
                return new SaveToCloudOutput
                {
                    IsOk = false,
                    Message = "One Drive not login"
                };
            }

            await _oneDriveClient.UploadAsync(cloudBkCollection);
            var oneDriveStatics = await _simpleDataStorage.GetOrDefaultAsync<OneDriveStatics>();
            oneDriveStatics.LastSuccessUploadTime = _clock.UtcNow;
            await _simpleDataStorage.SaveAsync(oneDriveStatics);
            var re = new SaveToCloudOutput
            {
                IsOk = true
            };
            return re;
        }
    }
}