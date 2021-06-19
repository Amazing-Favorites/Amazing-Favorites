using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public partial class SyncCloudJob : ISyncCloudJob
    {
        private readonly ILogger<SyncCloudJob> _logger;
        private readonly IBkManager _bkManager;
        private readonly IUserOptionsService _userOptionsService;
        private readonly ICloudService _cloudService;

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _jobHandler;

        public SyncCloudJob(
            ILogger<SyncCloudJob> logger,
            IBkManager bkManager,
            IUserOptionsService userOptionsService,
            ICloudService cloudService)
        {
            _logger = logger;
            _bkManager = bkManager;
            _userOptionsService = userOptionsService;
            _cloudService = cloudService;
        }

        public async ValueTask StartAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _jobHandler =  new[] {1L}.ToObservable()
                .Concat(Observable.Interval(TimeSpan.FromMinutes(10)))
                .Buffer(TimeSpan.FromSeconds(5), 50)
                .Where(x => x.Count > 0)
                .Select(x => x.First())
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        await RunSyncAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Faile");
                    }
                }))
                .Concat()
                .Subscribe();
        }

        private async Task RunSyncAsync()
        {
            var options = await _userOptionsService.GetOptionsAsync();
            if (options.CloudBkFeature?.Enabled == true)
            {
                var etagVersion = await _bkManager.GetEtagVersionAsync();
                var (hasChanged, output) = await _cloudService.GetCloudAsync(etagVersion);
                if (hasChanged)
                {
                    if (output!.EtagVersion > etagVersion)
                    {
                        // sync to local
                        await _bkManager.LoadCloudCollectionAsync(output.CloudBkCollection!);
                        LogSyncToLocal(_logger, output.EtagVersion, output.LastUpdateTime);
                    }
                    else if (output.EtagVersion < etagVersion)
                    {
                        // sync to cloud
                        var cloudBkCollection = await _bkManager.GetCloudBkCollectionAsync();
                        if (cloudBkCollection.Bks.Count > 0)
                        {
                            await _cloudService.SaveToCloudAsync(cloudBkCollection);
                            LogSyncToCloud(_logger,
                                cloudBkCollection.EtagVersion,
                                cloudBkCollection.LastUpdateTime);
                        }
                        else
                        {
                            LogNoBkFoundToSync(_logger);
                        }
                    }
                }
                else
                {
                    LogSameToCloud(_logger, etagVersion);
                }
            }
        }

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Failed to sync log",
            EventId = 1)]
        partial void LogErrorSync(ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "It is the same to cloud. etagVersion: {etagVersion}",
            EventId = 2)]
        partial void LogSameToCloud(ILogger logger, long etagVersion);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "There is no BK need sync to cloud",
            EventId = 3)]
        partial void LogNoBkFoundToSync(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Bk collection sync to local, etagVersion: {etagVersion}, lastUpdateTime: {lastUpdateTime}",
            EventId = 4)]
        partial void LogSyncToLocal(ILogger logger, long etagVersion, long lastUpdateTime);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Bk collection sync to cloud, etagVersion: {etagVersion}, lastUpdateTime: {lastUpdateTime}",
            EventId = 5)]
        partial void LogSyncToCloud(ILogger logger, long etagVersion, long lastUpdateTime);
    }
}