using System.Threading.Tasks;
using BlazorApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.Services.Configuration;

namespace Newbe.BookmarkManager.Services
{
    public class ShowWhatNewJob : IShowWhatNewJob
    {
        private readonly ILogger<ShowWhatNewJob> _logger;
        private readonly IApplicationInsights _applicationInsights;
        private readonly IIndexedDbRepo<AfMetadata, string> _afMetadataRepo;
        private readonly IOptions<StaticUrlOptions> _staticUrlOptions;
        private readonly INewNotification _newNotification;

        public ShowWhatNewJob(
            ILogger<ShowWhatNewJob> logger,
            IApplicationInsights applicationInsights,
            IIndexedDbRepo<AfMetadata, string> afMetadataRepo,
            IOptions<StaticUrlOptions> staticUrlOptions,
            INewNotification newNotification)
        {
            _logger = logger;
            _applicationInsights = applicationInsights;
            _afMetadataRepo = afMetadataRepo;
            _staticUrlOptions = staticUrlOptions;
            _newNotification = newNotification;
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
                    await _newNotification.NewReleaseAsync(new NewReleaseInput
                    {
                        Version = Consts.CurrentVersion,
                        WhatsNewUrl = whatsNew
                    });
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