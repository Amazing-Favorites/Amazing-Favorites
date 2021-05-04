using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtension.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public class BkDataHolder : IBkDataHolder
    {
        private readonly ILogger<BkDataHolder> _logger;
        private readonly IBkRepository _bkRepository;
        private readonly IClock _clock;
        private readonly Subject<DataChangeActionItem> _dataChangeActionSubject = new();
        private readonly Subject<long> _dataUpdatedSubject = new();

        private record DataChangeActionItem(TaskCompletionSource Tcs, Func<Task<bool>> Action);

        public BkDataHolder(
            ILogger<BkDataHolder> logger,
            IBkRepository bkRepository,
            IClock clock)
        {
            _logger = logger;
            _bkRepository = bkRepository;
            _clock = clock;
        }

        public Task PushDataChangeActionAsync(Func<Task<bool>> action)
        {
            var tcs = new TaskCompletionSource();
            _dataChangeActionSubject.OnNext(new DataChangeActionItem(tcs, action));
            return tcs.Task;
        }

        public BkEntityCollection Collection { get; private set; }

        public async ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> bks)
        {
            var bkDic = bks.ToLookup(x => x.Url, x => new Bk
            {
                Title = x.Title,
                TitleLastUpdateTime = _clock.UtcNow,
                Url = x.Url,
                LastCreateTime = x.DateAdded.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long) x.DateAdded.Value).ToUnixTimeSeconds()
                    : 0L
            });
            var bookmarksKeys = bkDic.Select(x => x.Key).ToHashSet();
            _logger.LogDebug("Found {Count} bookmark", bookmarksKeys.Count);
            Collection = await _bkRepository.GetLatestDataAsync();
            Collection.Bks ??= new();

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
            await SaveToStorageAsync();
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

        private async ValueTask SaveToStorageAsync()
        {
            var lastUpdatedTime = await _bkRepository.GetLateUpdateTimeAsync();
            Collection.LastUpdateTime = _clock.UtcNow;
            if (lastUpdatedTime < Collection.LastUpdateTime)
            {
                await _bkRepository.SaveAsync(Collection);
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
                OnDataReload?.Invoke(this, new EventArgs());
            }
        }

        private IDisposable _loadFromStorageHandler;

        public async ValueTask InitAsync()
        {
            await LoadFromStorageAsync();
            _loadFromStorageHandler = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(x => { LoadFromStorageAsync(); });

            _dataChangeActionSubject
                .Select(item => Observable.FromAsync(async () =>
                {
                    var (taskCompletionSource, action) = item;
                    try
                    {
                        var updateSuccess = await action.Invoke();
                        if (updateSuccess)
                        {
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
        }
    }
}