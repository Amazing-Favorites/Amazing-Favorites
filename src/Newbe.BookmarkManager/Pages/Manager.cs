using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Components;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.SimpleData;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Manager : IAsyncDisposable
    {
        [Inject] public IBkSearcher BkSearcher { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        [Inject] public ITagsManager TagsManager { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public IUserOptionsService UserOptionsService { get; set; }
        [Inject] public IBkEditFormData BkEditFormData { get; set; }
        [Inject] public IAfCodeService AfCodeService { get; set; }
        [Inject] public IManagePageNotificationService ManagePageNotificationService { get; set; }
        [Inject] public IOptions<StaticUrlOptions> StaticUrlOptions { get; set; }
        [Inject] public IOptions<DevOptions> DevOptions { get; set; }
        [Inject] public IRecordService RecordService { get; set; }
        [Inject] public IRecentSearchHolder RecentSearchHolder { get; set; }
        [Inject] public IAfEventHub AfEventHub { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public ISimpleDataStorage SimpleDataStorage { get; set; }
        [Inject] public IClock Clock { get; set; }
        private UserOptions _userOptions;

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

        private bool _searchInputLoading;
        private readonly Subject<string?> _searchSubject = new();
        private readonly Subject<bool> _altKeySubject = new();
        private readonly Subject<string> _updateFaviconSubject = new();
        private readonly Subject<OnSearchResultClickArgs> _userUrlClickSubject = new();
        private readonly List<IDisposable> _subjectHandlers = new();
        private readonly int _resultLimit = 10;
        private AutoCompleteSearch _search;

        private IEnumerable<string> SearchOptions
        {
            get
            {
                return RecentSearchHolder?.RecentSearch?.Items?.Select(x => x.Text)
                       ?? Array.Empty<string>();
            }
        }

        private string[] _allTags = Array.Empty<string>();
        private string _searchValue = null!;
        private bool _modalVisible;

        public record OnSearchResultClickArgs
        {
            public string SearchText { get; set; }
            public BkViewItem ClickItem { get; set; }
        }

        private void OnReceivedCommand(string command)
        {
            Logger.LogInformation("received command: {Command}", command);
            if (command == Consts.Commands.OpenManager)
            {
#pragma warning disable 4014
                FocusSearchBox();
#pragma warning restore 4014
            }
        }

        private async Task FocusSearchBox()
        {
            await InvokeAsync(async () =>
            {
                SearchValue = string.Empty;
                await _search.Focus();
                StateHasChanged();
            });
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _userOptions = await UserOptionsService.GetOptionsAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await RecentSearchHolder.LoadAsync();
                var lDotNetReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("DotNet.SetDotnetReference", lDotNetReference);
                _searchSubject
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .Select(x => x?.Trim())
                    .Subscribe(async args =>
                    {
                        _searchInputLoading = true;
                        StateHasChanged();
                        try
                        {
                            Logger.LogInformation("Search: {Args}", args!);
                            if (!string.IsNullOrWhiteSpace(args))
                            {
                                var afCode = args;
                                if (await AfCodeService.TryParseAsync(afCode, out var afCodeResult))
                                {
                                    if (afCodeResult != null)
                                    {
                                        await OnClickEdit(afCodeResult.Url, afCodeResult.Title, afCodeResult.Tags);
                                        SearchValue = string.Empty;
                                        return;
                                    }
                                }
                            }

                            var target = await BkSearcher.Search(args!, _resultLimit);
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

                var subjectHandler = _userUrlClickSubject.Subscribe(bk =>
                {
                    _updateFaviconSubject.OnNext(bk.ClickItem.Bk.Url);
                });
                _subjectHandlers.Add(subjectHandler);
                subjectHandler = _userUrlClickSubject.Subscribe(bk =>
                {
                    BkManager.AddClickAsync(bk.ClickItem.Bk.Url, 1);
                });
                _subjectHandlers.Add(subjectHandler);
                subjectHandler = _userUrlClickSubject
                    .Select(item =>
                        Observable.FromAsync(() => RecentSearchHolder.AddAsync(item.SearchText)))
                    .Concat()
                    .Subscribe();
                _subjectHandlers.Add(subjectHandler);
                subjectHandler = _userUrlClickSubject.Subscribe(item =>
                {
                    var bk = item.ClickItem;
                    RecordService.AddAsync(new UserClickRecord
                    {
                        Index = bk.LineIndex,
                        Url = bk.Bk.Url,
                        Title = bk.Bk.Title,
                        Search = SearchValue,
                        Tags = string.Join(",", ((IEnumerable<string>?)bk.Bk.Tags) ?? Array.Empty<string>())
                    });

                    InvokeAsync(() =>
                    {
                        SearchValue = string.Empty;
                        _searchSubject.OnNext(null);
                    });
                });
                _subjectHandlers.Add(subjectHandler);

                subjectHandler = _updateFaviconSubject
                    .Subscribe(url =>
                    {
                        Task.Run(async () =>
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
                                        { url, tabNow.FavIconUrl }
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        });
                    });
                _subjectHandlers.Add(subjectHandler);

                _allTags = await TagsManager.GetAllTagsAsync();

                await WebExtensions.Commands.OnCommand.AddListener(OnReceivedCommand);

                var (tabId, clickTime) = await SimpleDataStorage.GetOrDefaultAsync<LastUserClickIconTabData>();
                if (Clock.UtcNow - clickTime < TimeSpan.FromSeconds(30).TotalSeconds)
                {
                    Logger.LogInformation("{sss}", tabId);
                    Logger.LogInformation("{sss}", clickTime);
                    if (tabId > 0)
                    {
                        var tab = await WebExtensions.Tabs.Get(tabId);
                        if (tab != null)
                        {
                            await BkEditFormData.LoadAsync(tab.Url, tab.Title, Array.Empty<string>());
                            _modalVisible = true;
                            _returnTabId = tab.Id;
                            StateHasChanged();
                        }
                    }
                }

                await WebExtensions.Tabs.OnUpdated.AddListener(async (tabId, changeInfo, tab) =>
                {
                    await BkManager.AddClickAsync(changeInfo.Url, 1);
                    await InvokeAsync(() =>
                    {
                        SearchValue = _searchValue;
                        StateHasChanged();
                    });
                });
                await ManagePageNotificationService.RunAsync();
                AfEventHub.RegisterHandler<UserOptionSaveEvent>(HandleUserOptionSaveEvent);
                AfEventHub.RegisterHandler<TriggerEditBookmarkEvent>(HandleTriggerEditBookmarkEvent);
                await AfEventHub.EnsureStartAsync();
            }
        }

        private async Task HandleTriggerEditBookmarkEvent(TriggerEditBookmarkEvent evt)
        {
            Logger.LogInformation("Received : {Event}", evt);
            await BkEditFormData.LoadAsync(evt.Url, evt.Title, Array.Empty<string>());
            _modalVisible = true;
            _returnTabId = evt.TabId;
            StateHasChanged();
        }

        private Task HandleUserOptionSaveEvent(UserOptionSaveEvent arg)
        {
            return InvokeAsync(() =>
            {
                _userOptions = arg.UserOptions;
                StateHasChanged();
            });
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
                    await OpenSearchResult(number);
                }
            }
            else
            {
                _altKeySubject.OnNext(false);
            }
        }

        private async Task OpenSearchResult(string? number)
        {
            if (int.TryParse(number, out var selectIndex) &&
                selectIndex > 0 &&
                selectIndex < _targetBks.Length)
            {
                var arrayIndex = selectIndex - 1;
                Logger.LogInformation("Click keyboard to select {ArrayIndex} item", arrayIndex);
                await OnClickUrl(_targetBks[arrayIndex], default);
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

            _userUrlClickSubject.OnNext(new OnSearchResultClickArgs
            {
                ClickItem = bk,
                SearchText = SearchValue
            });
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

        #region Modal

        private bool _isFormLoading = false;

        private async Task OnClickEdit(string url, string title, string[]? tags = null)
        {
            try
            {
                _isFormLoading = true;
                tags ??= Array.Empty<string>();
                await BkEditFormData.LoadAsync(url, title, tags);
                _modalVisible = true;
            }
            finally
            {
                _isFormLoading = false;
            }
        }

        private async Task OnClickModalRemoveAsync()
        {
            try
            {
                _isFormLoading = true;
                await BkEditFormData.RemoveAsync();
                await CloseBkEditFormAsync();
            }
            finally
            {
                _isFormLoading = false;
            }
        }

        private void OnSharingButton()
        {
            OnClickSharing(BkEditFormData.Url);
        }

        private async Task OnClickModalSaveAsync()
        {
            try
            {
                _isFormLoading = true;
                await BkEditFormData.SaveAsync();
                await CloseBkEditFormAsync();
            }
            finally
            {
                _isFormLoading = false;
            }
        }

        private async Task OnClickModalCancelAsync()
        {
            await CloseBkEditFormAsync();
        }

        private int? _returnTabId;

        private async Task CloseBkEditFormAsync()
        {
            _modalVisible = false;
            StateHasChanged();
            SearchValue = _searchValue;
            try
            {
                if (_returnTabId != null)
                {
                    var tab = await WebExtensions.Tabs.Get(_returnTabId.Value);
                    if (tab != null)
                    {
                        await WebExtensions.Tabs.Update(tab.Id, new UpdateProperties
                        {
                            Active = true
                        });
                    }
                }
            }
            finally
            {
                _returnTabId = null;
            }
        }

        #endregion Modal

        #region AfCode

        private bool _afCodeSharingPanelVisible;
        private string _afCodeSharingUrl = string.Empty;

        private void OnClickSharing(string url)
        {
            _afCodeSharingUrl = url;
            _afCodeSharingPanelVisible = true;
        }

        #endregion AfCode

        public ValueTask DisposeAsync()
        {
            foreach (var subjectHandler in _subjectHandlers)
            {
                subjectHandler.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }
}