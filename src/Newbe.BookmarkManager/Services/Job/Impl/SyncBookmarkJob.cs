using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class SyncBookmarkJob : ISyncBookmarkJob
    {
        private readonly ILogger<SyncBookmarkJob> _logger;
        private readonly IBkDataHolder _bkDataHolder;
        private readonly IBookmarkDataHolder _bookmarkDataHolder;
        private readonly IClock _clock;
        private readonly Subject<long> _jobSubject = new();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _loadHandler;

        public SyncBookmarkJob(
            ILogger<SyncBookmarkJob> logger,
            IBkDataHolder bkDataHolder,
            IBookmarkDataHolder bookmarkDataHolder,
            IClock clock)
        {
            _logger = logger;
            _bkDataHolder = bkDataHolder;
            _bookmarkDataHolder = bookmarkDataHolder;
            _clock = clock;
        }

        public async ValueTask StartAsync()
        {
            _bookmarkDataHolder.StartAsync();
            _loadHandler = _jobSubject
                .Merge(Observable.Interval(TimeSpan.FromSeconds(10)))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        var bookmarkTreeNodes = _bookmarkDataHolder.Nodes;
                        await _bkDataHolder.AppendBookmarksAsync(bookmarkTreeNodes.Values);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed append bookmarks to storage");
                    }
                }))
                .Concat()
                .Subscribe(_ => { });
        }

        public ValueTask LoadNowAsync()
        {
            _jobSubject.OnNext(_clock.UtcNow);
            return ValueTask.CompletedTask;
        }
    }
}