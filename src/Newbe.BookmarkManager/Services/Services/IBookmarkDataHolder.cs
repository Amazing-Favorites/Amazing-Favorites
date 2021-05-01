using System.Collections.Generic;
using System.Threading.Tasks;
using WebExtension.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBookmarkDataHolder
    {
        IReadOnlyDictionary<string, BookmarkTreeNode> Nodes { get; }
        ValueTask StartAsync();
    }
}