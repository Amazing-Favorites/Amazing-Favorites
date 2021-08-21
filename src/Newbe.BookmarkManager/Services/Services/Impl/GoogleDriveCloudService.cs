using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.SimpleData;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public class GoogleDriveCloudService : ICloudService
    {
        private readonly IGoogleDriveClient _googleDriveClient;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;

        public GoogleDriveCloudService(
            IGoogleDriveClient googleDriveClient,
            ISimpleDataStorage simpleDataStorage,
            IClock clock)
        {
            _googleDriveClient = googleDriveClient;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            if (!await _googleDriveClient.TestAsync())
            {
                return new CloudBkStatus(false, new GetCloudOutput());
            }

            var fileDescription = await _googleDriveClient.GetFileDescriptionAsync();
            if (fileDescription == null)
            {
                return new CloudBkStatus(true, new GetCloudOutput()
                {
                    EtagVersion = 0,
                    LastUpdateTime = 0
                });
            }

            var cloudBkCollection = await _googleDriveClient.GetCloudDataAsync();
            return new CloudBkStatus(etagVersion != fileDescription.EtagVersion, new GetCloudOutput
            {
                EtagVersion = fileDescription.EtagVersion,
                LastUpdateTime = fileDescription.LastUpdateTime,
                CloudBkCollection = cloudBkCollection
            });
        }

        public async Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            if (!await _googleDriveClient.TestAsync())
            {
                return new SaveToCloudOutput
                {
                    IsOk = false,
                    Message = "google not login"
                };
            }

            await _googleDriveClient.UploadAsync(cloudBkCollection);
            var googleDriveStatics = await _simpleDataStorage.GetOrDefaultAsync<GoogleDriveStatics>();
            googleDriveStatics.LastSuccessUploadTime = _clock.UtcNow;
            await _simpleDataStorage.SaveAsync(googleDriveStatics);
            var re = new SaveToCloudOutput
            {
                IsOk = true
            };
            return re;
        }
    }
}