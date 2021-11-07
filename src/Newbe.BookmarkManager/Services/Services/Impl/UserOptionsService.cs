using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services.Configuration;

namespace Newbe.BookmarkManager.Services
{
    public class UserOptionsService : IUserOptionsService
    {
        private readonly IOptions<BaseUriOptions> _baseUriOptions;
        private readonly ILogger<UserOptionsService> _logger;
        private readonly IIndexedDbRepo<UserOptions, string> _userOptionsRepo;

        public UserOptionsService(
            IOptions<BaseUriOptions> baseUriOptions,
            ILogger<UserOptionsService> logger,
            IIndexedDbRepo<UserOptions, string> userOptionsRepo)
        {
            _baseUriOptions = baseUriOptions;
            _logger = logger;
            _userOptionsRepo = userOptionsRepo;
        }

        public async ValueTask<UserOptions> GetOptionsAsync()
        {
            var re = await _userOptionsRepo.GetSingleOneAsync();
            var isNew = false;
            if (re == null)
            {
                isNew = true;
                re = new UserOptions();
                _logger.LogInformation("There is no data found from repo, try to create a new one");
            }

            re = ApplyDefaultValue(re);
            if (isNew)
            {
                await SaveAsync(re);
            }

            return re;
        }

        public async Task SaveAsync(UserOptions options)
        {
            await _userOptionsRepo.UpsertAsync(options);
        }

        private UserOptions ApplyDefaultValue(UserOptions options)
        {
            var baseUriOptions = _baseUriOptions.Value;
            options.PinyinFeature ??= new PinyinFeature
            {
                Enabled = false,
                BaseUrl = baseUriOptions.PinyinApi
            };

            options.CloudBkFeature ??= new CloudBkFeature
            {
                Enabled = false,
                BaseUrl = baseUriOptions.CloudBkApi
            };

            options.HotTagsFeature ??= new HotTagsFeature
            {
                Enabled = true,
                ListCount = 10
            };

            options.ApplicationInsightFeature ??= new ApplicationInsightFeature
            {
                Enabled = false
            };

            options.OmniboxSuggestFeature ??= new OmniboxSuggestFeature
            {
                Enabled = true,
                SuggestCount = Consts.Omnibox.SuggestDefault
            };

            return options;
        }
    }
}