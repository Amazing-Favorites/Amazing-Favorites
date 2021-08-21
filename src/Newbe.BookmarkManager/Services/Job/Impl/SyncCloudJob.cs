﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public partial class SyncCloudJob : ISyncCloudJob
    {
        private readonly ILogger _logger;
        private readonly IBkManager _bkManager;
        private readonly IUserOptionsService _userOptionsService;
        private readonly ICloudServiceFactory _cloudServiceFactory;

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _jobHandler;

        public SyncCloudJob(
            ILogger<SyncCloudJob> logger,
            IBkManager bkManager,
            IUserOptionsService userOptionsService,
            ICloudServiceFactory cloudServiceFactory)
        {
            _logger = logger;
            _bkManager = bkManager;
            _userOptionsService = userOptionsService;
            _cloudServiceFactory = cloudServiceFactory;
        }

        public async ValueTask StartAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _jobHandler = new[] { 1L }.ToObservable()
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
                        _logger.LogError(e, "Failed");
                    }
                }))
                .Concat()
                .Subscribe();
        }

        private async Task RunSyncAsync()
        {
            var cloudService = await _cloudServiceFactory.CreateAsync();
            var options = await _userOptionsService.GetOptionsAsync();
            if (options is
                {
                    AcceptPrivacyAgreement: true,
                    CloudBkFeature:
                    {
                        Enabled: true,
                        AccessToken: not null,
                        BaseUrl: not null
                    }
                })
            {
                var etagVersion = await _bkManager.GetEtagVersionAsync();
                var (hasChanged, output) = await cloudService.GetCloudAsync(etagVersion);
                if (hasChanged)
                {
                    if (output!.EtagVersion > etagVersion)
                    {
                        // sync to local
                        await _bkManager.LoadCloudCollectionAsync(output.CloudBkCollection!);
                        LogSyncToLocal(output.EtagVersion, output.LastUpdateTime);
                    }
                    else if (output.EtagVersion < etagVersion)
                    {
                        // sync to cloud
                        var cloudBkCollection = await _bkManager.GetCloudBkCollectionAsync();
                        if (cloudBkCollection.Bks.Count > 0)
                        {
                            await cloudService.SaveToCloudAsync(cloudBkCollection);
                            LogSyncToCloud(
                                cloudBkCollection.EtagVersion,
                                cloudBkCollection.LastUpdateTime);
                        }
                        else
                        {
                            LogNoBkFoundToSync();
                        }
                    }
                }
                else
                {
                    LogSameToCloud(etagVersion);
                }
            }
        }

        [LoggerMessage(Level = LogLevel.Error,
            Message = "Failed to sync log",
            EventId = 1)]
        partial void LogErrorSync(Exception ex);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "It is the same to cloud. etagVersion: {etagVersion}",
            EventId = 2)]
        partial void LogSameToCloud(long etagVersion);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "There is no BK need sync to cloud",
            EventId = 3)]
        partial void LogNoBkFoundToSync();

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Bk collection sync to local, etagVersion: {etagVersion}, lastUpdateTime: {lastUpdateTime}",
            EventId = 4)]
        partial void LogSyncToLocal(long etagVersion, long lastUpdateTime);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "Bk collection sync to cloud, etagVersion: {etagVersion}, lastUpdateTime: {lastUpdateTime}",
            EventId = 5)]
        partial void LogSyncToCloud(long etagVersion, long lastUpdateTime);
    }
}