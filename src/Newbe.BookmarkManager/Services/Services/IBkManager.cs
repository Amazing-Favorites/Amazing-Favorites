using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;
using WebExtension.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        ValueTask AddClickAsync(string url, int moreCount);
        ValueTask RestoreAsync();

        ValueTask RemoveTagAsync(string url, string tag);
        ValueTask<bool> AddTagAsync(string url, string tag);
        ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags);
        ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls);
        ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> nodes);
        Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection);
        Task<CloudBkCollection> GetCloudBkCollectionAsync();
        Task DeleteAsync(string url);
        Task UpdateTitleAsync(string url, string title);
        Task<long> GetEtagVersionAsync();
        Task<Bk?> Get(string url);
    }
}