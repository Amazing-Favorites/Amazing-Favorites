using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services.Configuration;
using WebExtension.Net.Storage;
using static Newbe.BookmarkManager.Services.Consts.StorageKeys;

namespace Newbe.BookmarkManager.Services
{
    public class UserOptionsRepository : IUserOptionsRepository
    {
        private readonly IOptions<BaseUriOptions> _baseUriOptions;
        private readonly ILogger<UserOptionsRepository> _logger;
        private readonly IStorageApi _storageApi;

        public UserOptionsRepository(
            IOptions<BaseUriOptions> baseUriOptions,
            ILogger<UserOptionsRepository> logger,
            IStorageApi storageApi)
        {
            _baseUriOptions = baseUriOptions;
            _logger = logger;
            _storageApi = storageApi;
        }

        public async ValueTask<UserOptions> GetOptionsAsync()
        {
            var local = await _storageApi.GetLocal();
            var jsonElement = await local.Get(UserOptionsName);
            var json = string.Empty;
            if (jsonElement.TryGetProperty(UserOptionsName, out var elValue))
            {
                json = elValue.ToString();
            }

            _logger.LogDebug("Found data in storage : {Json}", json);

            UserOptions re;
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogInformation("There is no data found from repo, try to create a new one");
                re = CreateDefault();
                await SaveAsync(re);
            }
            else
            {
                re = JsonSerializer.Deserialize<UserOptions>(json);
            }

            return re;
        }

        public async ValueTask SaveAsync(UserOptions options)
        {
            var local = await _storageApi.GetLocal();
            var json = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(options));
            _logger.LogInformation("Data save: {Json}", json);
            await local.Set(new Dictionary<string, object>
            {
                {UserOptionsName, json},
            });
        }

        private UserOptions CreateDefault()
        {
            var baseUriOptions = _baseUriOptions.Value;
            return new UserOptions
            {
                PinyinFeature = new PinyinFeature
                {
                    Enabled = false,
                    BaseUrl = baseUriOptions.PinyinApi
                }
            };
        }
    }
}