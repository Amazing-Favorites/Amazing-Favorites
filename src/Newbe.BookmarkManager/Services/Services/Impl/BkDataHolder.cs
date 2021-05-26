using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtension.Net.Bookmarks;
using WebExtension.Net.Storage;

namespace Newbe.BookmarkManager.Services
{
    public class BkDataHolder : IBkDataHolder
    {
        private readonly ILogger<BkDataHolder> _logger;
        private readonly IBkRepository _bkRepository;
        private readonly IClock _clock;
        private readonly IUrlHashService _urlHashService;
        private readonly IStorageApi _storageApi;
        private readonly Subject<DataChangeActionItem> _dataChangeActionSubject = new();
        private readonly Subject<long> _dataUpdatedSubject = new();
        private readonly Subject<long> _loadFromStorageSubject = new();
        private long _lastUpdateTime;

        private record DataChangeActionItem(TaskCompletionSource Tcs, Func<Task<bool>> Action);

        public BkDataHolder(
            ILogger<BkDataHolder> logger,
            IBkRepository bkRepository,
            IClock clock,
            IUrlHashService urlHashService,
            IStorageApi storageApi)
        {
            _logger = logger;
            _bkRepository = bkRepository;
            _clock = clock;
            _urlHashService = urlHashService;
            _storageApi = storageApi;
        }

        public Task PushDataChangeActionAsync(Func<Task<bool>> action)
        {
            var tcs = new TaskCompletionSource();
            _dataChangeActionSubject.OnNext(new DataChangeActionItem(tcs, action));
            return tcs.Task;
        }

        public void UpdateEtagVersion(long etagVersion)
        {
            Collection.EtagVersion = etagVersion;
            MarkUpdated();
        }

        public BkEntityCollection Collection { get; private set; }

        public async ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> bks)
        {
            var bkDic = bks.ToLookup(x => x.Url, x => new Bk
            {
                Title = x.Title,
                TitleLastUpdateTime = _clock.UtcNow,
                Url = x.Url,
                UrlHash = _urlHashService.GetHash(x.Url),
                LastCreateTime = x.DateAdded.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long) x.DateAdded.Value).ToUnixTimeSeconds()
                    : 0L
            });
            var bookmarksKeys = bkDic.Select(x => x.Key).ToHashSet();
            _logger.LogDebug("Found {Count} bookmark", bookmarksKeys.Count);

            foreach (var grouping in bkDic)
            {
                var url = grouping.Key;
                var node = grouping.First();
                if (Collection.Bks.TryGetValue(url, out var bk))
                {
                    if (bk.Title != node.Title)
                    {
                        await this.PushDataChangeActionAsync(() =>
                        {
                            bk.Title = node.Title;
                            bk.TitleLastUpdateTime = _clock.UtcNow;
                        });
                    }
                }
            }

            var newKeys = bookmarksKeys.Except(Collection.Bks.Keys).ToArray();

            if (newKeys.Length > 0)
            {
                await this.PushDataChangeActionAsync(() =>
                {
                    foreach (var key in newKeys)
                    {
                        var bk = bkDic[key].First();
                        Collection.Bks.Add(bk.Url, bk);
                    }

                    _logger.LogInformation("There are {Count} links new, add to storage", newKeys.Length);
                });
            }
        }


        public event EventHandler<EventArgs> OnDataReload;

        private void MarkUpdated()
        {
            _dataUpdatedSubject.OnNext(_clock.UtcNow);
        }

        public async ValueTask SaveNowAsync()
        {
            await SaveToStorageAsync(true);
        }

        public async ValueTask RestoreAsync()
        {
            Collection = new BkEntityCollection
            {
                Version = 1
            };
            await SaveToStorageAsync();
            _logger.LogInformation("Data has been restore!");
        }

        private async ValueTask SaveToStorageAsync(bool force = false)
        {
            Collection.LastUpdateTime = _clock.UtcNow;
            if (force || _lastUpdateTime < Collection.LastUpdateTime)
            {
                await _bkRepository.SaveAsync(Collection);
                _lastUpdateTime = Collection.LastUpdateTime;
                _logger.LogInformation(
                    "Data saved to storage, count: {Count}, LastUpdatedTime: {LastUpdatedTime}",
                    Collection.Bks.Count,
                    Collection.LastUpdateTime);
            }
        }

        private async ValueTask LoadFromStorageAsync()
        {
            if (Collection == null)
            {
                await LoadCoreAsync();
                _logger.LogInformation("first time load from storage");
                return;
            }

            var lastUpdateTime = await _bkRepository.GetLateUpdateTimeAsync();
            if (lastUpdateTime > Collection.LastUpdateTime)
            {
                await LoadCoreAsync();
                _logger.LogInformation("data modified from storage, load it");
            }

            async Task LoadCoreAsync()
            {
                Collection = await _bkRepository.GetLatestDataAsync();
                _lastUpdateTime = Collection.LastUpdateTime;
                OnDataReload?.Invoke(this, new EventArgs());
            }
        }

        private IDisposable _loadFromStorageHandler;
        private bool _initialized;

        public async ValueTask StartAsync()
        {
            if (!_initialized)
            {
                _lastUpdateTime = await _bkRepository.GetLateUpdateTimeAsync();
                await LoadFromStorageAsync();
                _loadFromStorageHandler = _loadFromStorageSubject
                    .Buffer(TimeSpan.FromSeconds(1), 50)
                    .Where(x => x.Count > 0)
                    .Select(_ => Observable.FromAsync(async () => await LoadFromStorageAsync()))
                    .Concat()
                    .Subscribe();

                await _storageApi.OnChanged.AddListener((obj, area) =>
                {
                    if (area == "local" &&
                        obj is JsonElement el &&
                        el.TryGetProperty(Consts.StorageKeys.BookmarksDataLastUpdatedTime, out var timeJson) &&
                        timeJson.TryGetProperty("newValue", out var timeValue))
                    {
                        var newTime = timeValue.GetInt64();
                        if (newTime > Collection.LastUpdateTime)
                        {
                            _logger.LogInformation(
                                "BkCollection reload since updated last update time changed with value {NewTime}",
                                newTime);
                            _loadFromStorageSubject.OnNext(newTime);
                        }
                        else
                        {
                            _logger.LogDebug("NewTime is {NewTime} not greater than {NowTime}",
                                newTime,
                                Collection.LastUpdateTime);
                        }
                    }
                });

                _dataChangeActionSubject
                    .Select(item => Observable.FromAsync(async () =>
                    {
                        var (taskCompletionSource, action) = item;
                        try
                        {
                            var updateSuccess = await action.Invoke();
                            if (updateSuccess)
                            {
                                Collection.EtagVersion++;
                                _logger.LogInformation("Bk collection updated success");
                                MarkUpdated();
                            }

                            taskCompletionSource.SetResult();
                        }
                        catch (Exception e)
                        {
                            taskCompletionSource.SetException(e);
                        }
                    }))
                    .Concat()
                    .Subscribe(_ => { });

                _dataUpdatedSubject
                    .Buffer(TimeSpan.FromSeconds(1), 50)
                    .Where(x => x.Count > 0)
                    .Select(_ => Observable.FromAsync(async () => { await SaveToStorageAsync(); }))
                    .Concat()
                    .Subscribe(_ => { });
                _initialized = true;
            }
            else
            {
                _logger.LogInformation("{Name} has been initialized, skip to StartAsync", nameof(BkDataHolder));
            }
        }
    }
}