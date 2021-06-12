using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Components;
using Newbe.BookmarkManager.Services;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Popup
    {
        private FormModel _formModel = new();
        [Inject] private IBkManager BkManager { get; set; }
        [Inject] private IBookmarksApi BookmarksApi { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var allTags = await BkManager.GetAllTagsAsync();
                _formModel.AllTags = allTags;
                var tabs = await WebExtensions.Tabs.Query(new QueryInfo
                {
                    Active = true,
                    CurrentWindow = true
                });
                var tab = tabs.FirstOrDefault();
                if (tab != null &&
                    !string.IsNullOrWhiteSpace(tab.Url) &&
                    !string.IsNullOrWhiteSpace(tab.Title))
                {
                    _formModel.Url = tab.Url;
                    _formModel.Title = tab.Title;
                    var bookmarkTreeNodes = await BookmarksApi.Search(new
                    {
                        url = tab.Url
                    });
                    var bookmarkTreeNode = bookmarkTreeNodes.FirstOrDefault();
                    if (bookmarkTreeNode == null)
                    {
                        _formModel.IsFirstAdded = true;
                        var folderNode = await CreateAmazingFavoriteFolderAsync();
                        var treeNode = await BookmarksApi.Create(new CreateDetails
                        {
                            Title = tab.Title,
                            Url = tab.Url,
                            ParentId = folderNode.Id,
                        });
                        _formModel.BookmarkTreeNode = treeNode;
                        Logger.LogInformation("New bk added");
                    }
                    else
                    {
                        _formModel.BookmarkTreeNode = bookmarkTreeNode;
                    }

                    var bk = await BkManager.Get(tab.Url);
                    if (bk != null)
                    {
                        _formModel.Tags = bk.Tags?.ToHashSet() ?? new HashSet<string>();
                    }

                    Logger.LogInformation("Bk found");

                    _formModel.IsLoading = false;
                    _formModel.IsShowEditor = true;
                }
                else
                {
                    Logger.LogInformation("this tab is missing url or title, can not be add to bookmarks");
                    _formModel.IsAvailable = false;
                    _formModel.IsShowEditor = false;
                }

                StateHasChanged();
            }
        }

        private async Task<BookmarkTreeNode> CreateAmazingFavoriteFolderAsync()
        {
            var bookmarkTreeNodes = await BookmarksApi.Search(new
            {
                title = Consts.AmazingFavoriteFolderName
            });
            var oldNode = bookmarkTreeNodes.FirstOrDefault();
            if (oldNode is not {Type: BookmarkTreeNodeType.Folder})
            {
                var newNode = await BookmarksApi.Create(new CreateDetails
                {
                    Title = Consts.AmazingFavoriteFolderName,
                });

                Logger.LogInformation("{FolderName} not found, created", Consts.AmazingFavoriteFolderName);
                return newNode;
            }

            Logger.LogInformation("{FolderName} found", Consts.AmazingFavoriteFolderName);

            return oldNode;
        }

        private async Task OpenManager()
        {
            await WebExtensions.Tabs.ActiveOrOpenManagerAsync();
        }

        private async Task RemoveBookmarkAsync()
        {
            await BookmarksApi.Remove(_formModel.BookmarkTreeNode.Id);
            _formModel.IsRemoved = true;
            _formModel.IsShowEditor = false;
            StateHasChanged();
        }

        private async Task UpdateAsync()
        {
            _formModel.IsUpdateButtonLoading = true;
            var node = _formModel.BookmarkTreeNode;
            if (_formModel.Title != node.Title)
            {
                var newNode = await BookmarksApi.Update(node.Id, new Changes
                {
                    Title = _formModel.Title
                });
                _formModel.BookmarkTreeNode = newNode;
                await BkManager.AppendBookmarksAsync(new[] {newNode});
            }

            await BkManager.UpdateTagsAsync(_formModel.Url, _formModel.Tags);
            _formModel.IsUpdateSuccess = true;

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                _formModel.IsUpdateSuccess = false;
                _formModel.IsUpdateButtonLoading = false;
                StateHasChanged();
            });
        }

        private void OnRemovingTag(string tag)
        {
            _formModel.Tags.Remove(tag);
            StateHasChanged();
        }

        private void OnNewTagsAddAsync(TagInput.NewTagArgs args)
        {
            foreach (var tag in args.Tags)
            {
                _formModel.Tags.Add(tag);
            }
        }
    }

    public class FormModel
    {
        public string Title { get; set; } = string.Empty;
        public HashSet<string> Tags { get; set; } = new();
        public string Url { get; set; } = string.Empty;
        public BookmarkTreeNode BookmarkTreeNode { get; set; } = null!;
        public bool IsFirstAdded { get; set; }
        public bool IsRemoved { get; set; } = false;
        public bool IsShowEditor { get; set; }
        public bool IsLoading { get; set; } = true;
        public bool IsAvailable { get; set; } = true;
        public string NewTag { get; set; }
        public string[] AllTags { get; set; } = Array.Empty<string>();

        public bool IsUpdateSuccess { get; set; }
        public bool IsUpdateButtonLoading { get; set; }
    }
}