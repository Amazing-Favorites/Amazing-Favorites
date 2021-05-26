using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;
using WebExtension.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        Task InitAsync();
        ValueTask AddClickAsync(string url, int moreCount);
        ValueTask RestoreAsync();

        ValueTask RemoveTagAsync(string url, string tag);
        ValueTask<bool> AddTagAsync(string url, string tag);
        ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags);
        ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls);
        ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> nodes);
        Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection);
        CloudBkCollection GetCloudBkCollection();
        long GetEtagVersion();
        Bk? Get(string url);
    }
}