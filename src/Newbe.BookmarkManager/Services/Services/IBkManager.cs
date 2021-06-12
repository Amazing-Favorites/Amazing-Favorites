using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        ValueTask AddClickAsync(string url, int moreCount);
        ValueTask RestoreAsync();

        ValueTask RemoveTagAsync(string url, string tag);
        ValueTask<bool> AppendTagAsync(string url, params string[]? tags);
        ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags);
        ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls);
        ValueTask AppendBookmarksAsync(IEnumerable<BookmarkNode> nodes);
        Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection);
        Task<CloudBkCollection> GetCloudBkCollectionAsync();
        Task DeleteAsync(string url);
        Task UpdateTitleAsync(string url, string title);
        Task<long> GetEtagVersionAsync();
        Task<Bk?> Get(string url);

        Task<string[]> GetAllTagsAsync();
    }

    public class BookmarkNode
    {
        public BookmarkNode()
        {
        }

        public BookmarkNode(BookmarkTreeNode node)
        {
            Title = node.Title;
            Url = node.Url;
            DateAdded = node.DateAdded;
        }

        public double? DateAdded { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public List<string> Tags { get; set; }
    }
}