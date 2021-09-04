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
            var maxLevelCount = 5;
            var i = 0;
            while (currentNode != null)
            {
                yield return currentNode;
                if (++i > maxLevelCount)
                {
                    yield break;
                }

                if (!string.IsNullOrEmpty(currentNode.ParentId))
                {
                    var nodes = await bookmarksApi.Get(currentNode.ParentId);
                    currentNode = nodes?.FirstOrDefault();
                }
            }
        }
    }
}