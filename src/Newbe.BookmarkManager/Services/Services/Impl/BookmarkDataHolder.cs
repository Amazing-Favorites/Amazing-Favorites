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

        public IReadOnlyDictionary<string, BookmarkTreeNode> Nodes => _nodes;

        private Dictionary<string, BookmarkTreeNode> _nodes = new();

        public BookmarkDataHolder(
            ILogger<BookmarkDataHolder> logger,
            IBookmarksApi bookmarksApi)
        {
            _logger = logger;
            _bookmarksApi = bookmarksApi;
        }

        private bool _initialized;

        public async ValueTask StartAsync()
        {
            if (!_initialized)
            {
                await LoadAllBookmarkAsync();
                _logger.LogInformation("bookmarks load from user storage, count: {Count}", Nodes.Count);

                await _bookmarksApi.OnRemoved.AddListener(async (s, info) =>
                {
                    await LoadAllBookmarkAsync();
                    _logger.LogInformation("Bookmark data reload since item removed");
                });
                await _bookmarksApi.OnChanged.AddListener(async (s, info) =>
                {
                    await LoadAllBookmarkAsync();
                    _logger.LogInformation("Bookmark data reload since item changed");
                });
                await _bookmarksApi.OnCreated.AddListener((s, node) =>
                {
                    if (!string.IsNullOrWhiteSpace(node.Url) &&
                        !string.IsNullOrWhiteSpace(node.Title) &&
                        !Nodes.ContainsKey(node.Url))
                    {
                        _nodes[node.Url] = node;
                    }

                    _logger.LogInformation("Bookmark data reload since item added");
                });

                var observable = _loadSubject.Merge(Observable.Interval(TimeSpan.FromMinutes(10)))
                    .Select(_ => Observable.FromAsync(async () =>
                    {
                        await LoadAllBookmarkAsync();
                        _logger.LogInformation("Bookmark data reload since time matched");
                    }))
                    .Concat()
                    .Subscribe(_ => { });
                _loadHandler = observable;
                _initialized = true;
            }
            else
            {
                _logger.LogInformation("{Name} has been initialized, skip to StartAsync", nameof(BookmarkDataHolder));
            }
        }

        private async Task LoadAllBookmarkAsync()
        {
            try
            {
                var items = await GetAllBookmarkAsync();
                if (items != null)
                {
                    _nodes = items.ToLookup(x => x.Url)
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
                    node.Unmodifiable != BookmarkTreeNodeUnmodifiable.Managed)
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