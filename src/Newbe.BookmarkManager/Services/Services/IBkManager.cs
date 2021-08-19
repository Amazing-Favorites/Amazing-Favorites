using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.WebApi;
using WebExtensions.Net.Bookmarks;
using static Newbe.BookmarkManager.Services.Ai.Events;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        [Insight(EventName = BkClickEvent)]
        Task AddClickAsync(string url, int moreCount);

        [Insight(EventName = BkRestoreEvent)]
        Task RestoreAsync();

        [Insight(EventName = BkTagRemoveEvent)]
        Task RemoveTagAsync(string url, string tag);

        [Insight(EventName = BkTagAppendEvent)]
        Task<bool> AppendTagAsync(string url, params string[]? tags);

        [Insight(EventName = BkTagUpdateEvent)]
        Task UpdateTagsAsync(string url, string title, IEnumerable<string> tags);

        Task UpdateFavIconUrlAsync(Dictionary<string, string> urls);

        [Insight(EventName = BkBookmarksSyncEvent)]
        Task AppendBookmarksAsync(IEnumerable<BookmarkNode> nodes);

        [Insight(EventName = BkCloudToLocalEvent)]
        Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection);

        Task<CloudBkCollection> GetCloudBkCollectionAsync();

        [Insight(EventName = BkDeleteEvent)]
        Task DeleteAsync(string url);

        [Insight(EventName = BkTitleUpdateEvent)]
        Task UpdateTitleAsync(string url, string title);

        Task<long> GetEtagVersionAsync();
        Task<Bk?> Get(string url);
        Task<Dictionary<string, int>> GetTagRelatedCountAsync();
    }

    public record UserClickRecord
    {
        public string Search { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public int Index { get; set; }
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