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
        public HashSet<string> Tags { get; set; } = new();
        public string[] AllTags { get; set; }
        private string _oldTitle;

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

        public async Task LoadAsync(string url, string title, string[] tags)
        {
            Url = url;
            var bk = await _bkManager.Get(url);
            Title = title;
            if (bk != null)
            {
                _oldTitle = bk.Title;
                Tags = bk.Tags?.ToHashSet() ?? new HashSet<string>();
            }
            else
            {
                _oldTitle = string.Empty;
                Tags = new HashSet<string>(tags);
            }

            AllTags = await _tagsManager.GetAllTagsAsync();
        }

        public async Task SaveAsync()
        {
            _logger.LogInformation("On click save: {Data}", this);
            var url = Url;
            var title = Title;
            await _bkManager.UpdateTagsAsync(url, title, Tags);

            var bookmarkTreeNodes = await _bookmarksApi.Search(new
            {
                url = Url
            });
            var bookmarkTreeNode = bookmarkTreeNodes.FirstOrDefault();
            if (bookmarkTreeNode == null)
            {
                var folderNode = await CreateAmazingFavoriteFolderAsync();
                await _bookmarksApi.Create(new CreateDetails
                {
                    Title = Title,
                    Url = Url,
                    ParentId = folderNode.Id,
                });
            }
            else
            {
                if (_oldTitle != title)
                {
                    await _bookmarksApi.Update(bookmarkTreeNode.Id, new Changes
                    {
                        Title = title,
                        Url = url
                    });
                    _logger.LogInformation("Bookmark updated, new title: {Title}", title);
                }
            }
        }

        private async Task<BookmarkTreeNode> CreateAmazingFavoriteFolderAsync()
        {
            var bookmarkTreeNodes = await _bookmarksApi.Search(new
            {
                title = Consts.AmazingFavoriteFolderName
            });
            var oldNode = bookmarkTreeNodes.FirstOrDefault();
            if (oldNode is not { Type: BookmarkTreeNodeType.Folder })
            {
                var newNode = await _bookmarksApi.Create(new CreateDetails
                {
                    Title = Consts.AmazingFavoriteFolderName,
                });

                _logger.LogInformation("{FolderName} not found, created", Consts.AmazingFavoriteFolderName);
                return newNode;
            }

            _logger.LogInformation("{FolderName} found", Consts.AmazingFavoriteFolderName);

            return oldNode;
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