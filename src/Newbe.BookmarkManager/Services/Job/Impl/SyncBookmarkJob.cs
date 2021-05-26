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
        private readonly IBookmarkDataHolder _bookmarkDataHolder;
        private readonly IBkManager _bkManager;
        private readonly IClock _clock;
        private readonly Subject<long> _jobSubject = new();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable? _loadHandler;

        public SyncBookmarkJob(
            ILogger<SyncBookmarkJob> logger,
            IBookmarkDataHolder bookmarkDataHolder,
            IBkManager bkManager,
            IClock clock)
        {
            _logger = logger;
            _bookmarkDataHolder = bookmarkDataHolder;
            _bkManager = bkManager;
            _clock = clock;
        }

        public async ValueTask StartAsync()
        {
            await _bkManager.InitAsync();
            await _bookmarkDataHolder.StartAsync();
            _loadHandler = _jobSubject
                .Merge(Observable.Interval(TimeSpan.FromSeconds(10)))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        var bookmarkTreeNodes = _bookmarkDataHolder.Nodes;
                        await _bkManager.AppendBookmarksAsync(bookmarkTreeNodes.Values);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed append bookmarks to storage");
                    }
                }))
                .Concat()
                .Subscribe(_ => { });
            _jobSubject.OnNext(_clock.UtcNow);
        }
    }
}