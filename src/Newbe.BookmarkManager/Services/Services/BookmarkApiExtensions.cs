using System.Collections.Generic;
using System.Linq;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public static class BookmarkApiExtensions
    {
        public static async IAsyncEnumerable<BookmarkTreeNode> GetAllParentAsync(this IBookmarksApi bookmarksApi,
            string id)
        {
            var node = await bookmarksApi.Get(id);
            var currentNode = node?.FirstOrDefault();
            while (currentNode != null)
            {
                yield return currentNode;
                var nodes = await bookmarksApi.Get(currentNode.ParentId);
                currentNode = nodes?.FirstOrDefault();
            }
        }
    }
}