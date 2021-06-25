using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Components;
using Newbe.BookmarkManager.Services;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Manager
    {
        [Inject] public IBkSearcher BkSearcher { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        [Inject] public ITagsManager TagsManager { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public IUserOptionsService UserOptionsService { get; set; }
        [Inject] public IBkEditFormData BkEditFormData { get; set; }

        private BkViewItem[] _targetBks = Array.Empty<BkViewItem>();

        private string SearchValue
        {
            get => _searchValue;
            set
            {
                _searchValue = value;
                _searchSubject?.OnNext(value);
            }
        }

        private UserOptions _userOptions;
        private bool _searchInputLoading;
        private readonly Subject<string?> _searchSubject = new();
        private readonly Subject<bool> _altKeySubject = new();
        private readonly Subject<string> _updateFaviconSubject = new();
        private IDisposable _searchPlaceHolderHandler;
        private IDisposable _updateFaviconHandler;
        private readonly int _resultLimit = 10;
        private bool _controlPanelVisible;
        private Search _search;
        private string[] _allTags = Array.Empty<string>();
        private string _searchValue;
        private bool _modalVisible;

        [JSInvokable]
        public async Task OnReceivedCommand(string command)
        {
            Logger.LogInformation("received command: {Command}", command);
            if (command == Consts.Commands.OpenManager)
            {
                SearchValue = string.Empty;
                await _search.Focus();
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await ImportAsync("content/manager_keyboard.js");
            var lDotNetReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("GLOBAL.SetDotnetReference", lDotNetReference);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                _searchSubject
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .Select(x => x?.Trim())
                    .Subscribe(async args =>
                    {
                        _searchInputLoading = true;
                        StateHasChanged();
                        try
                        {
                            Logger.LogInformation("Search: {Args}", args);
                            var target = await BkSearcher.Search(args, _resultLimit);
                            _targetBks = Map(target);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, "Error when search");
                        }
                        finally
                        {
                            _searchInputLoading = false;
                        }

                        StateHasChanged();
                    });
                SearchValue = _searchValue;

                _altKeySubject.DistinctUntilChanged()
                    .Subscribe(show =>
                    {
                        var length = Math.Min(_targetBks.Length, 9);
                        for (var i = 0; i < length; i++)
                        {
                            var bk = _targetBks[i];
                            bk.ShowIndex = show;
                        }

                        StateHasChanged();
                    });

                _updateFaviconHandler = _updateFaviconSubject
                    .Subscribe(async url =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            var tabs = await WebExtensions.Tabs.Query(new QueryInfo
                            {
                                Url = url
                            });
                            var tabNow = tabs.FirstOrDefault();
                            if (tabNow != null &&
                                !string.IsNullOrWhiteSpace(tabNow.FavIconUrl))
                            {
                                Logger.LogInformation("success to get favicon url");
                                await BkManager.UpdateFavIconUrlAsync(new Dictionary<string, string>
                                {
                                    {url, tabNow.FavIconUrl}
                                });
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                _allTags = await TagsManager.GetAllTagsAsync();
                _userOptions = await UserOptionsService.GetOptionsAsync();
            }
        }

        private BkViewItem[] Map(SearchResultItem[] items)
        {
            var re = CreateItem().ToArray();
            return re;

            IEnumerable<BkViewItem> CreateItem()
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    yield return new BkViewItem(item.Bk)
                    {
                        LineIndex = i + 1
                    };
                }
            }
        }

        [JSInvokable]
        public async Task OnSearchInputKeydown(string code, bool altKey)
        {
            if (altKey)
            {
                _altKeySubject.OnNext(true);
                if (code.StartsWith("Numpad") || code.StartsWith("Digit"))
                {
                    var number = code[^1..];
                    if (int.TryParse(number, out var selectIndex) &&
                        selectIndex > 0 &&
                        selectIndex < _targetBks.Length)
                    {
                        var arrayIndex = selectIndex - 1;
                        Logger.LogInformation("Click keyboard to select {ArrayIndex} item", arrayIndex);
                        await OnClickUrl(_targetBks[arrayIndex], default);
                    }
                }
            }
            else
            {
                _altKeySubject.OnNext(false);
            }
        }

        [JSInvokable]
        public Task OnSearchInputKeyup(string code, bool altKey)
        {
            if (code == "AltLeft")
            {
                _altKeySubject.OnNext(false);
            }

            return Task.CompletedTask;
        }

        private async Task OnClickUrl(BkViewItem bk, MouseEventArgs? e)
        {
            var url = bk.Bk.Url;
            if (e?.CtrlKey == true)
            {
                await WebExtensions.Tabs.OpenAsync(url);
            }
            else
            {
                await WebExtensions.Tabs.ActiveOrOpenAsync(url);
            }

            _updateFaviconSubject.OnNext(url);
            await BkManager.AddClickAsync(url, 1);
            SearchValue = string.Empty;
            _searchSubject.OnNext(null);
        }

        private async Task OnRemovingTag(Bk bk, string tag)
        {
            await BkManager.RemoveTagAsync(bk.Url, tag);
            _allTags = await TagsManager.GetAllTagsAsync();
            SearchValue = _searchValue;
        }

        private void OnClickTag(string tagKey)
        {
            TagsManager.AddCountAsync(tagKey, 1);
            if (string.IsNullOrWhiteSpace(SearchValue))
            {
                SearchValue = string.Empty;
            }

            var tagSearchValue = $"t:{tagKey}";
            if (!SearchValue.Contains(tagSearchValue))
            {
                SearchValue = $"{SearchValue} {tagSearchValue}";
            }
        }

        private async Task OnNewTagsAddAsync(BkViewItem bk, string[] newTags)
        {
            _allTags = await TagsManager.GetAllTagsAsync();
            await BkManager.AppendTagAsync(bk.Bk.Url, newTags);
            SearchValue = _searchValue;
        }

        private async Task OpenHelp()
        {
            await WebExtensions.Tabs.OpenAsync("https://af.newbe.pro/");
        }

        private async Task OpenControlPanel()
        {
            _controlPanelVisible = true;
        }

        private async Task OnClickResumeFactorySetting()
        {
            await BkManager.RestoreAsync();
            SearchValue = _searchValue;
            _controlPanelVisible = false;
        }

        private void OnUserOptionSave(ControlPanel.OnUserOptionSaveArgs args)
        {
            _userOptions = args.Options;
        }

        #region Modal

        private async Task OnClickEdit(string url, string title)
        {
            await BkEditFormData.LoadAsync(url, title);
            _modalVisible = true;
        }

        private async Task OnClickModalRemoveAsync()
        {
            await BkEditFormData.RemoveAsync();
            CloseBkEditForm();
        }

        private async Task OnClickModalSaveAsync()
        {
            await BkEditFormData.SaveAsync();
            CloseBkEditForm();
        }

        private async Task OnClickModalCancelAsync()
        {
            CloseBkEditForm();
        }

        private void CloseBkEditForm()
        {
            _modalVisible = false;
            SearchValue = _searchValue;
        }

        #endregion
    }
}