﻿using System;
using System.Collections;
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
                var nodes = GetAllChildren(new[] { info.Node });
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
                    var tags = new HashSet<string>();
                    var nodes = _bookmarksApi.GetAllParentAsync(node.ParentId);
                    var deepth = 0;
                    var parentOffset = 0;
                    await foreach (var parentNode in nodes)
                    {
                        if (deepth == 0)
                        {
                            parentOffset = parentNode.Index ?? 0;
                        }
                        if (!string.IsNullOrWhiteSpace(parentNode.Title)
                            && !Consts.IsReservedBookmarkFolder(parentNode.Title))
                        {
                            deepth++;
                            tags.Add(parentNode.Title);
                        }
                    }
                    await _bkManager.AppendBookmarksAsync(new[] {new BookmarkNode(node)
                    {
                        Deepth = deepth + 1,
                        ParentNodeOffset = parentOffset,
                        Tags = tags.ToList()
                    }});
                }

                _logger.LogInformation("Bookmark data reload since item added");
            });

            await _bookmarksApi.OnMoved.AddListener(async (s, info) =>
            {
                var tags = new HashSet<string>();
                var node = (await _bookmarksApi.Get(s)).FirstOrDefault();
                var nodes = _bookmarksApi.GetAllParentAsync(node.ParentId);
                var deepth = 0;
                var parentOffset = 0;
                await foreach (var parentNode in nodes)
                {
                    if (deepth == 0)
                    {
                        parentOffset = parentNode.Index ?? 0;
                    }
                    if (!string.IsNullOrWhiteSpace(parentNode.Title)
                        && !Consts.IsReservedBookmarkFolder(parentNode.Title))
                    {
                        deepth++;
                        tags.Add(parentNode.Title);
                    }
                }
                if (!string.IsNullOrWhiteSpace(node.Url) &&
                    !string.IsNullOrWhiteSpace(node.Title))
                {
                    await _bkManager.UpdatePositionAsync(node.Url, tags.ToList(), deepth + 1, parentOffset, node.Index ?? 0);
                }
            });

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var all = await GetAllBookmarkAsync();
                _logger.LogInformation("Found {Count} bookmarks, try to load them", all.Count);
                // make init order look like bookmarks tree
                await _bkManager.AppendBookmarksAsync(all);
            });
        }

        private async ValueTask<List<BookmarkNode>> GetAllBookmarkAsync()
        {
            var bookmarkTreeNodes = await _bookmarksApi.GetTree();
            if (bookmarkTreeNodes == null)
            {
                return new List<BookmarkNode>();
            }

            return GetAllChildren(bookmarkTreeNodes);
        }

        private List<BookmarkNode> GetAllChildren(IEnumerable<BookmarkTreeNode> bookmarkTreeNodes)
        {
            var queue = new Queue<BkItem>(bookmarkTreeNodes.Select(x => new BkItem(x, new BookmarkNode(x)
            {
                Tags = new List<string>()
            })));
            var result = new List<BookmarkNode>();
            int deepth = 0;
            while (queue.Any())
            {
                int level = queue.Count();

                while (level-- > 0)
                {
                    var item = queue.Dequeue();
                    var (node, bookmarkNode) = item;
                    if (!string.IsNullOrWhiteSpace(node.Url) &&
                        !string.IsNullOrWhiteSpace(node.Title) &&
                        node.Unmodifiable != BookmarkTreeNodeUnmodifiable.Managed)
                    {
                        _logger.LogInformation($"deepth sum {deepth}");
                        bookmarkNode.Deepth = deepth;
                        result.Add(bookmarkNode);
                    }
                    if (node.Children != null)
                    {
                        _logger.LogInformation($"folder  Id:{node.Id},ParentId:{node.ParentId},Title:{node.Title}_index : {node.Index}_child : {node.Children.Count()}");
                        foreach (var child in node.Children)
                        {
                            var tags = new List<string>(bookmarkNode.Tags);
                            if (!string.IsNullOrWhiteSpace(node.Title)
                                && !Consts.IsReservedBookmarkFolder(node.Title))
                            {
                                tags.Add(node.Title);
                            }

                            queue.Enqueue(new BkItem(child, new BookmarkNode(child)
                            {
                                ParentNodeOffset = node.Index ?? 0,
                                Tags = tags,
                            }));
                        }

                    }
                }
                deepth++;
            }

            return result;
        }

        public record BkItem(BookmarkTreeNode TreeNode, BookmarkNode Node);
    }
}