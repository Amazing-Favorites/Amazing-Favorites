using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Services
{
    public class SyncCloudStatusCheckJob : ISyncCloudStatusCheckJob
    {
        private readonly IAfEventHub _afEventHub;
        private readonly ILogger<SyncCloudStatusCheckJob> _logger;
        private readonly IUserOptionsService _userOptionsService;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;
        private readonly INewNotification _newNotification;
        private readonly Subject<long> _eventSubject = new();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _jobHandler;

        public SyncCloudStatusCheckJob(
            IAfEventHub afEventHub,
            ILogger<SyncCloudStatusCheckJob> logger,
            IUserOptionsService userOptionsService,
            ISimpleDataStorage simpleDataStorage,
            IClock clock,
            INewNotification newNotification)
        {
            _afEventHub = afEventHub;
            _logger = logger;
            _userOptionsService = userOptionsService;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
            _newNotification = newNotification;
        }

        public ValueTask StartAsync()
        {
            _jobHandler = _eventSubject
                .Concat(Observable.Interval(TimeSpan.FromHours(1)))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        await RunSyncAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed");
                    }
                }))
                .Concat()
                .Subscribe();
            _eventSubject.OnNext(1);
            return ValueTask.CompletedTask;
        }

        private async Task RunSyncAsync()
        {
            var userOptions = await _userOptionsService.GetOptionsAsync();
            if (userOptions.CloudBkFeature is
                {
                    Enabled: true,
                })
            {
                var time = TimeSpan.FromDays(1);
                switch (userOptions.CloudBkFeature.CloudBkProviderType)
                {
                    case CloudBkProviderType.GoogleDrive:
                    case CloudBkProviderType.OneDrive:
                        var data = await _simpleDataStorage.GetOrDefaultAsync<CloudBkSyncStatics>();
                        if (data.LastSyncTime != null &&
                            _clock.UtcNow > data.LastSyncTime + time.TotalSeconds)
                        {
                            await _newNotification.SyncDataWithCloudAsync(new SyncDataWithCloudInput
                            {
                                LastSyncTime = data.LastSyncTime.Value
                            });
                        }

                        break;
                }
            }
        }
    }
}