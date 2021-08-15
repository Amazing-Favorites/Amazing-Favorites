using System.Threading.Tasks;
using BlazorApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.Services.Configuration;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public class ShowWelcomeJob : IShowWelcomeJob
    {
        private readonly ILogger<ShowWelcomeJob> _logger;
        private readonly IApplicationInsights _applicationInsights;
        private readonly IIndexedDbRepo<AfMetadata, string> _afMetadataRepo;
        private readonly ITabsApi _tabsApi;
        private readonly IOptions<StaticUrlOptions> _staticUrlOptions;

        public ShowWelcomeJob(
            ILogger<ShowWelcomeJob> logger,
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

            if (!afMetadata.WelcomeShown)
            {
                var welcomeUrl = _staticUrlOptions.Value.Welcome;
                if (!string.IsNullOrEmpty(welcomeUrl))
                {
                    await _applicationInsights.TrackEvent(Events.AfWelcomeShown);
                    await _tabsApi.ActiveOrOpenAsync(welcomeUrl);
                    _logger.LogInformation("Welcome shown");
                }

                afMetadata.WelcomeShown = true;
                await _afMetadataRepo.UpsertAsync(afMetadata);
            }
            else
            {
                _logger.LogDebug("Welcome was shown before");
            }
        }
    }
}