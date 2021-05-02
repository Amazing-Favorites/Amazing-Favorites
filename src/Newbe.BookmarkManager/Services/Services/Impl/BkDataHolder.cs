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

        private record DataChangeActionItem(TaskCompletionSource Tcs, Func<Task> Action);

        public BkDataHolder(
            ILogger<BkDataHolder> logger,
            IBkRepository bkRepository,
            IClock clock)
        {
            _logger = logger;
            _bkRepository = bkRepository;
            _clock = clock;
            _dataChangeActionSubject
                .Select(item => Observable.FromAsync(async () =>
                {
                    var (taskCompletionSource, action) = item;
                    try
                    {
                        await action.Invoke();
                        taskCompletionSource.SetResult();
                        MarkUpdated();
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
                    }
                }))
                .Concat()
                .Subscribe(_ => { });
        }

        public Task PushDataChangeActionAsync(Func<Task> action)
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
                        await PushDataChangeActionAsync(() =>
                        {
                            bk.Title = node.Title;
                            return Task.CompletedTask;
                        });
                    }
                }
            }

            var newKeys = bookmarksKeys.Except(Collection.Bks.Keys).ToArray();

            if (newKeys.Length > 0)
            {
                await PushDataChangeActionAsync(() =>
                {
                    foreach (var key in newKeys)
                    {
                        var bk = bkDic[key].First();
                        Collection.Bks.Add(bk.Url, bk);
                    }

                    _logger.LogInformation("There are {Count} links new, add to storage", newKeys.Length);
                    return Task.CompletedTask;
                });
            }

            await SaveToStorageAsync();
        }

        private bool _isUpdated;
        private IDisposable _saveHandler;
        private IDisposable _loadFromStorageHandler;


        public event EventHandler<EventArgs> OnDataReload;

        public void MarkUpdated()
        {
            _isUpdated = true;
        }

        public async ValueTask SaveNowAsync()
        {
            MarkUpdated();
            await SaveToStorageAsync();
        }

        public async ValueTask RestoreAsync()
        {
            Collection = new BkEntityCollection
            {
                Version = 1
            };
            MarkUpdated();
            await SaveToStorageAsync();
            _logger.LogInformation("Data has been restore!");
        }

        private async ValueTask SaveToStorageAsync()
        {
            if (_isUpdated)
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

                _isUpdated = false;
            }
        }

        private async ValueTask LoadFromStorageAsync()
        {
            var bkEntityCollection = await _bkRepository.GetLatestDataAsync();
            if (Collection == null)
            {
                Collection = bkEntityCollection;
            }

            if (bkEntityCollection.LastUpdateTime > Collection.LastUpdateTime)
            {
                Collection = bkEntityCollection;
                OnDataReload?.Invoke(this, new EventArgs());
                _logger.LogInformation("data modified from storage, load it");
            }
        }

        public async ValueTask InitAsync()
        {
            await LoadFromStorageAsync();
            _saveHandler = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(x => { SaveToStorageAsync(); });
            _loadFromStorageHandler = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(x => { LoadFromStorageAsync(); });
        }
    }
}