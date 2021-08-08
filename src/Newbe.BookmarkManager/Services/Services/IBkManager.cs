using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.WebApi;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        [Insight(EventName = "Bk Click Event")]
        Task AddClickAsync(string url, int moreCount);

        [Insight(EventName = "Bk Restore Event")]
        Task RestoreAsync();

        [Insight(EventName = "Bk Tag Remove Event")]
        Task RemoveTagAsync(string url, string tag);

        [Insight(EventName = "Bk Tag Append Event")]
        Task<bool> AppendTagAsync(string url, params string[]? tags);

        [Insight(EventName = "Bk Tag Update Event")]
        Task UpdateTagsAsync(string url, string title, IEnumerable<string> tags);

        Task UpdateFavIconUrlAsync(Dictionary<string, string> urls);

        [Insight(EventName = "Bk Bookmarks Sync Event")]
        Task AppendBookmarksAsync(IEnumerable<BookmarkNode> nodes);

        [Insight(EventName = "Bk Cloud To Local Event")]
        Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection);

        Task<CloudBkCollection> GetCloudBkCollectionAsync();

        [Insight(EventName = "Bk Delete Event")]
        Task DeleteAsync(string url);

        [Insight(EventName = "Bk Title Update Event")]
        Task UpdateTitleAsync(string url, string title);

        Task<long> GetEtagVersionAsync();
        Task<Bk?> Get(string url);
        Task<Dictionary<string, int>> GetTagRelatedCountAsync();
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