using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public class SyncBookmarkJob : ISyncBookmarkJob
    {
        private readonly ILogger<SyncBookmarkJob> _logger;
        private readonly IBkManager _bkManager;
        private readonly IBookmarksApi _bookmarksApi;

        public SyncBookmarkJob(
            ILogger<SyncBookmarkJob> logger,
            IBkManager bkManager,
            IBookmarksApi bookmarksApi)
        {
            _logger = logger;
            _bkManager = bkManager;
            _bookmarksApi = bookmarksApi;
        }

        public async ValueTask StartAsync()
        {
            await _bookmarksApi.OnRemoved.AddListener(async (s, info) =>
            {
                var nodes = GetAllChildren(new []{info.Node});
                foreach (var bookmarkTreeNode in nodes)
                {
                    await _bkManager.DeleteAsync(bookmarkTreeNode.Url);
                }

                _logger.LogInformation("Bookmark data reload since item removed");
            });
            await _bookmarksApi.OnChanged.AddListener(async (s, info) =>
            {
                if (!string.IsNullOrWhiteSpace(info.Url) &&
                    !string.IsNullOrWhiteSpace(info.Title))
                {
                    await _bkManager.UpdateTitleAsync(info.Url, info.Title);
                }

                _logger.LogInformation("Bookmark data reload since item changed");
            });
            await _bookmarksApi.OnCreated.AddListener(async (s, node) =>
            {
                if (!string.IsNullOrWhiteSpace(node.Url) &&
                    !string.IsNullOrWhiteSpace(node.Title))
                {
                    await _bkManager.AppendBookmarksAsync(new[] {node});
                }

                _logger.LogInformation("Bookmark data reload since item added");
            });

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var all = await GetAllBookmarkAsync();
                _logger.LogInformation("Found {Count} bookmarks, try to load them", all.Count);
                // make init order look like bookmarks tree
                await _bkManager.AppendBookmarksAsync(all.OrderByDescending(x => x.DateAdded));
            });
        }

        private async ValueTask<List<BookmarkTreeNode>> GetAllBookmarkAsync()
        {
            var bookmarkTreeNodes = await _bookmarksApi.GetTree();
            if (bookmarkTreeNodes == null)
            {
                return new List<BookmarkTreeNode>();
            }

            return GetAllChildren(bookmarkTreeNodes);
        }

        private static List<BookmarkTreeNode> GetAllChildren(IEnumerable<BookmarkTreeNode> bookmarkTreeNodes)
        {
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