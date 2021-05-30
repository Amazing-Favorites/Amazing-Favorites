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
using Newbe.BookmarkManager.Services;
using WebExtension.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Manager
    {
        [Inject] public IBkSearcher BkSearcher { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public IUserOptionsService UserOptionsService { get; set; }

        public class ModalModel
        {
            public bool Visible { get; set; }
            public PinyinFeature PinyinFeature { get; set; }
            public CloudBkFeature CloudBkFeature { get; set; }
        }

        private BkViewItem[] _targetBks = Array.Empty<BkViewItem>();

        private int _searchTipIndex = 0;

        private readonly string[] _searchTips =
        {
            "Welcome to visit https://af.newbe.pro to find full doc about this extensions",
            "Press alt + number in search box to select from search result",
            "Click tag to search with the specified tag",
            "Search a keyword with t: mean that you want to search a tag. e.g. t:book matches url with book tag",
            "You can enable pinyin search support in control panel",
        };

        private string _searchValue;
        private bool _searchInputLoading;
        private readonly Subject<string?> _searchSubject = new();
        private readonly Subject<bool> _altKeySubject = new();
        private readonly Subject<string> _updateFaviconSubject = new();
        private IDisposable _searchPlaceHolderHandler;
        private IDisposable _updateFaviconHandler;
        private readonly int _resultLimit = 10;
        private ModalModel _modal = new();
        private Search _search;

        [JSInvokable]
        public async Task OnReceivedCommand(string command)
        {
            Logger.LogInformation("received command: {Command}", command);
            if (command == Consts.Commands.OpenManager)
            {
                _searchValue = string.Empty;
                _searchSubject.OnNext(_searchValue);
                await _search.Ref.FocusAsync();
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
                    .Throttle(TimeSpan.FromMilliseconds(500))
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
                _searchSubject.OnNext(_searchValue);

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

                _searchPlaceHolderHandler =
                    Observable.Interval(TimeSpan.FromSeconds(5))
                        .Subscribe(x =>
                        {
                            _searchTipIndex += 1;
                            _searchTipIndex %= _searchTips.Length;
                            StateHasChanged();
                        });
                _updateFaviconHandler = _updateFaviconSubject
                    .Subscribe(async url =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            var tabs = await WebExtension.Tabs.Query(new QueryInfo
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

        private void OnSearchInput(ChangeEventArgs item)
        {
            _searchSubject.OnNext(item.Value?.ToString());
        }

        private async Task OnSearchInputKeydown(KeyboardEventArgs obj)
        {
            if (obj.AltKey)
            {
                _altKeySubject.OnNext(true);
                if (obj.Code.StartsWith("Numpad") || obj.Code.StartsWith("Digit"))
                {
                    var number = obj.Code[^1..];
                    if (int.TryParse(number, out var selectIndex) &&
                        selectIndex > 0 &&
                        selectIndex < _targetBks.Length)
                    {
                        var arrayIndex = selectIndex - 1;
                        Logger.LogInformation("Click keyboard to select {ArrayIndex} item", arrayIndex);
                        await OnClickUrl(_targetBks[arrayIndex]);
                    }
                }
            }
            else
            {
                _altKeySubject.OnNext(false);
            }
        }

        private void OnSearchInputKeyup(KeyboardEventArgs obj)
        {
            if (obj.Code == "AltLeft")
            {
                _altKeySubject.OnNext(false);
            }
        }

        private async Task OnClickUrl(BkViewItem bk)
        {
            var url = bk.Bk.Url;
            await WebExtension.Tabs.Create(new CreateProperties
            {
                Url = url
            });
            _updateFaviconSubject.OnNext(url);
            await BkManager.AddClickAsync(url, 1);
            _searchValue = null;
            _searchSubject.OnNext(null);
        }

        private async Task OnRemovingTag(Bk bk, string tag)
        {
            await BkManager.RemoveTagAsync(bk.Url, tag);
            _searchSubject.OnNext(_searchValue);
        }

        private void OnClickTag(Bk bk, string tagKey)
        {
            if (string.IsNullOrWhiteSpace(_searchValue))
            {
                _searchValue = string.Empty;
            }

            var tagSearchValue = $"t:{tagKey}";
            if (!_searchValue.Contains(tagSearchValue))
            {
                _searchValue = $"{_searchValue} {tagSearchValue}";
            }

            _searchSubject.OnNext(_searchValue);
        }

        private async Task OnCreatingTag(BkViewItem bk)
        {
            var bkNewTag = bk.NewTag;
            await BkManager.AddTagAsync(bk.Bk.Url, bkNewTag);
            _searchSubject.OnNext(_searchValue);

            bk.NewTagInputVisible = false;
            bk.NewTag = string.Empty;
        }

        private async Task OnClickResumeFactorySetting()
        {
            await BkManager.RestoreAsync();
            _searchSubject.OnNext(_searchValue);
            CloseControlPanel();
        }

        private Task OnClickDumpDataAsync()
        {
            // TODO
            // var json = JsonSerializer.Serialize(BkDataHolder.Collection);
            // Logger.LogInformation(json);
            return Task.CompletedTask;
        }

        private void CloseControlPanel()
        {
            _modal.Visible = false;
        }

        private async Task HandleUserOptionsOk(MouseEventArgs e)
        {
            await UserOptionsService.SaveAsync(new UserOptions
            {
                PinyinFeature = _modal.PinyinFeature,
                CloudBkFeature = _modal.CloudBkFeature
            });
            CloseControlPanel();
        }

        private async Task HandleUserOptionsCancel(MouseEventArgs e)
        {
            CloseControlPanel();
        }

        private async Task OpenControlPanel()
        {
            await LoadUserOptions();
            _modal.Visible = true;
        }

        private async Task LoadUserOptions()
        {
            var options = await UserOptionsService.GetOptionsAsync();
            _modal.PinyinFeature = options.PinyinFeature;
            _modal.CloudBkFeature = options.CloudBkFeature;
        }
    }
}