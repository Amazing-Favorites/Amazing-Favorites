using System.Threading.Tasks;
using BlazorApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.Services.Configuration;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public class ShowWhatNewJob : IShowWhatNewJob
    {
        private readonly ILogger<ShowWhatNewJob> _logger;
        private readonly IApplicationInsights _applicationInsights;
        private readonly IIndexedDbRepo<AfMetadata, string> _afMetadataRepo;
        private readonly ITabsApi _tabsApi;
        private readonly IOptions<StaticUrlOptions> _staticUrlOptions;

        public ShowWhatNewJob(
            ILogger<ShowWhatNewJob> logger,
            IApplicationInsights applicationInsights,
            IIndexedDbRepo<AfMetadata, string> afMetadataRepo,
            ITabsApi tabsApi,
            IOptions<StaticUrlOptions> staticUrlOptions)
        {
            _logger = logger;
            _applicationInsights = applicationInsights;
            _afMetadataRepo = afMetadataRepo;
            _tabsApi = tabsApi;
            _staticUrlOptions = staticUrlOptions;
        }

        public async ValueTask StartAsync()
        {
            var afMetadata = await _afMetadataRepo.GetSingleOneAsync() ?? new AfMetadata();

            if (afMetadata.WhatsNewVersion != Consts.CurrentVersion)
            {
                var whatsNew = _staticUrlOptions.Value.WhatsNew;
                if (!string.IsNullOrEmpty(whatsNew))
                {
                    await _applicationInsights.TrackEvent(Events.AfWhatsNewShown);
                    await _tabsApi.ActiveOrOpenAsync(whatsNew);
                    _logger.LogInformation("Whats new shown");
                }

                afMetadata.WhatsNewVersion = Consts.CurrentVersion;
                await _afMetadataRepo.UpsertAsync(afMetadata);
            }
            else
            {
                _logger.LogDebug("Whats new was shown before");
            }
        }
    }
}