using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Manager
    {
        [Inject] public IBkSearcher BkSearcher { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        [Inject] public IBkDataHolder BkDataHolder { get; set; }
        [Inject] public ISyncBookmarkJob SyncBookmarkJob { get; set; }

        private BkViewItem[] _targetBks = Array.Empty<BkViewItem>();

        private int _searchTipIndex = 0;

        private readonly string[] _searchTips =
        {
            "Press alt + number in search box to select from search result",
            "Click tag to search with the specified tag",
            "Search a keyword with t: mean that you want to search a tag. e.g. t:book matches url with book tag"
        };

        private string _searchValue;
        private bool _searchInputLoading;
        private readonly Subject<string> _searchSubject = new();
        private readonly Subject<bool> _altKeySubject = new();
        private readonly Subject<string> _updateFaviconSubject = new();
        private bool _controlPanelVisible;
        private IDisposable _searchPlaceHolderHandler;
        private IDisposable _updateFaviconHandler;
        private readonly int _resultLimit = 10;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await BkDataHolder.InitAsync();
            BkDataHolder.OnDataReload += BkDataHolderOnOnDataReload;
            await SyncBookmarkJob.StartAsync();
            _searchSubject
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Select(x => x?.Trim())
                .Subscribe(args =>
                {
                    _searchInputLoading = true;
                    StateHasChanged();
                    try
                    {
                        Logger.LogInformation("Search: {Args}", args);
                        var target = BkSearcher.Search(args, _resultLimit);
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
                        var tabs = await WebExtension.Tabs.Query(new
                        {
                            url = url
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

        private void BkDataHolderOnOnDataReload(object sender, EventArgs e)
        {
            _searchSubject.OnNext(_searchValue);
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
            await WebExtension.Tabs.Create(new
            {
                url = url
            });
            _updateFaviconSubject.OnNext(url);
            await BkManager.AddClickAsync(url, 1);
            _searchValue = null;
            _searchSubject.OnNext(null);
        }


        private async Task OnRemovingTag(Bk bk, string tag)
        {
            await BkManager.RemoveTagAsync(bk.Url, tag);
            StateHasChanged();
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
            StateHasChanged();

            bk.NewTagInputVisible = false;
            bk.NewTag = string.Empty;
            _searchSubject.OnNext(_searchValue);
        }

        private async Task OnClickResumeFactorySetting()
        {
            await BkManager.RestoreAsync();
            _searchSubject.OnNext(_searchValue);
            ClockControlPanel();
        }

        private Task OnClickDumpDataAsync()
        {
            var json = JsonSerializer.Serialize(BkDataHolder.Collection);
            Logger.LogInformation(json);
            return Task.CompletedTask;
        }

        private void ClockControlPanel()
        {
            _controlPanelVisible = false;
        }
    }
}