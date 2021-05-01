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
    public class BookmarkDataHolder : IBookmarkDataHolder
    {
        private readonly ILogger<BookmarkDataHolder> _logger;
        private readonly IBookmarksApi _bookmarksApi;
        private readonly Subject<long> _loadSubject = new();
        private IDisposable _loadHandler;

        public IReadOnlyDictionary<string, BookmarkTreeNode> Nodes { get; private set; } =
            new Dictionary<string, BookmarkTreeNode>();

        public BookmarkDataHolder(
            ILogger<BookmarkDataHolder> logger,
            IBookmarksApi bookmarksApi)
        {
            _logger = logger;
            _bookmarksApi = bookmarksApi;
        }

        public async ValueTask StartAsync()
        {
            await LoadAllBookmarkAsync();
            _logger.LogInformation("bookmarks load from user storage, count: {Count}", Nodes.Count);
            var observable = _loadSubject.Merge(Observable.Interval(TimeSpan.FromSeconds(5)))
                .Select(_ => Observable.FromAsync(LoadAllBookmarkAsync))
                .Concat()
                .Subscribe(_ => { });
            _loadHandler = observable;
        }

        private async Task LoadAllBookmarkAsync()
        {
            try
            {
                var items = await GetAllBookmarkAsync();
                if (items != null)
                {
                    Nodes = items.ToLookup(x => x.Url)
                        .ToDictionary(x => x.Key, x => x.First());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get bookmarks");
            }
        }

        private async ValueTask<List<BookmarkTreeNode>> GetAllBookmarkAsync()
        {
            var bookmarkTreeNodes = await _bookmarksApi.GetTree();
            var queue = new Queue<BookmarkTreeNode>(bookmarkTreeNodes);
            var result = new List<BookmarkTreeNode>();
            while (queue.TryDequeue(out var node))
            {
                if (!string.IsNullOrWhiteSpace(node.Url) &&
                    !string.IsNullOrWhiteSpace(node.Title) &&
                    node.Unmodifiable == BookmarkTreeNodeUnmodifiable.Managed)
                {
                    result.Add(node);
                }

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return result;
        }
    }
}