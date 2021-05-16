using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtension.Net.Storage;
using static Newbe.BookmarkManager.Services.Consts.StorageKeys;

namespace Newbe.BookmarkManager.Services
{
    public class BkRepository : IBkRepository
    {
        private readonly ILogger<BkRepository> _logger;
        private readonly IStorageApi _storageApi;

        public BkRepository(
            ILogger<BkRepository> logger,
            IStorageApi storageApi)
        {
            _logger = logger;
            _storageApi = storageApi;
        }

        public async ValueTask<long> GetLateUpdateTimeAsync()
        {
            var local = await _storageApi.GetLocal();
            var jsonElement = await local.Get(BookmarksDataLastUpdatedTime);
            if (jsonElement.TryGetProperty(BookmarksDataLastUpdatedTime, out var elValue))
            {
                if (elValue.TryGetInt64(out var time))
                {
                    return time;
                }
            }

            return 0;
        }

        public async ValueTask<BkEntityCollection> GetLatestDataAsync()
        {
            var local = await _storageApi.GetLocal();
            var jsonElement = await local.Get(BookmarksData);
            var json = string.Empty;
            if (jsonElement.TryGetProperty(BookmarksData, out var elValue))
            {
                json = elValue.ToString();
            }

            _logger.LogDebug("Found data in storage : {Json}", json);
            BkEntityCollection re;
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogInformation("There is no data found from repo, try to create a new one");
                re = new BkEntityCollection
                {
                    Version = 1
                };
                await SaveAsync(re);
            }
            else
            {
                re = JsonSerializer.Deserialize<BkEntityCollection>(json);
            }

            return re;
        }

        public async ValueTask SaveAsync(BkEntityCollection collection)
        {
            var local = await _storageApi.GetLocal();
            var json = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(collection));
            _logger.LogDebug("Data save: {Json}", json);
            await local.Set(new Dictionary<string, object>
            {
                {BookmarksData, json},
                {BookmarksDataLastUpdatedTime, collection.LastUpdateTime}
            });
        }
    }
}