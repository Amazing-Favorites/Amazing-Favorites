using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public record BkEditFormData : IBkEditFormData
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string OldTitle { get; set; }
        public HashSet<string> Tags { get; set; } = new();
        public string[] AllTags { get; set; }

        private readonly ILogger<BkEditFormData> _logger;
        private readonly IBkManager _bkManager;
        private readonly ITagsManager _tagsManager;
        private readonly IBookmarksApi _bookmarksApi;

        public BkEditFormData(
            ILogger<BkEditFormData> logger,
            IBkManager bkManager,
            ITagsManager tagsManager,
            IBookmarksApi bookmarksApi)
        {
            _logger = logger;
            _bkManager = bkManager;
            _tagsManager = tagsManager;
            _bookmarksApi = bookmarksApi;
        }


        public async Task LoadAsync(string url, string title)
        {
            Url = url;
            Title = title;
            OldTitle = title;
            var bk = await _bkManager.Get(url);
            if (bk != null)
            {
                Tags = bk.Tags?.ToHashSet() ?? new HashSet<string>();
            }

            AllTags = await _tagsManager.GetAllTagsAsync();
        }

        public async Task SaveAsync()
        {
            _logger.LogInformation("On click save: {Data}", this);
            var url = Url;
            var title = Title;
            await _bkManager.UpdateTagsAsync(url, Tags);
            if (OldTitle != title)
            {
                await _bkManager.UpdateTitleAsync(url, title);
                var bookmarkTreeNodes = await _bookmarksApi.Search(new
                {
                    url = Url
                });
                var node = bookmarkTreeNodes.FirstOrDefault();
                if (node != null)
                {
                    await _bookmarksApi.Update(node.Id, new Changes
                    {
                        Title = title,
                        Url = url
                    });
                    _logger.LogInformation("Bookmark updated, new title: {Title}", title);
                }
            }
        }

        public async Task RemoveAsync()
        {
            var bookmarkTreeNodes = await _bookmarksApi.Search(new
            {
                url = Url
            });
            var node = bookmarkTreeNodes.FirstOrDefault();
            if (node != null)
            {
                await _bookmarksApi.Remove(node.Id);
            }
            
            await _bkManager.DeleteAsync(Url);
        }
    }
}