﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Components;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Manager : IAsyncDisposable
    {
        private JsModuleLoader _moduleLoader;
        [Inject] public IBkSearcher BkSearcher { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        [Inject] public ITagsManager TagsManager { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public IUserOptionsService UserOptionsService { get; set; }
        [Inject] public IBkEditFormData BkEditFormData { get; set; }
        [Inject] public IAfCodeService AfCodeService { get; set; }
        [Inject] public NotificationService Notice { get; set; }
        [Inject] public IOptions<StaticUrlOptions> StaticUrlOptions { get; set; }

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
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                _moduleLoader = new JsModuleLoader(JsRuntime);
                await _moduleLoader.LoadAsync("/content/manager_keyboard.js");
                var userOptions = await UserOptionsService.GetOptionsAsync();
                if (userOptions.ApplicationInsightFeature?.Enabled == true)
                {
                    await _moduleLoader.LoadAsync("/content/ai.js");
                }

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
                                    { url, tabNow.FavIconUrl }
                                });
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                _allTags = await TagsManager.GetAllTagsAsync();
                _userOptions = userOptions;

                await WebExtensions.Runtime.OnMessage.AddListener((o, sender, arg3) =>
                {
                    HandleNewBookmarkAddedEvent(o);
                    return true;
                });

                var editTabIdStr = QueryString(NavigationManager, "editTabId");
                if (int.TryParse(editTabIdStr, out var editTabId))
                {
                    if (editTabId > 0)
                    {
                        var tab = await WebExtensions.Tabs.Get(editTabId);
                        if (tab != null)
                        {
                            await BkEditFormData.LoadAsync(tab.Url, tab.Title, Array.Empty<string>());
                            _modalVisible = true;
                            _returnTabId = tab.Id;
                            StateHasChanged();
                        }
                    }
                }
                await AccessTokenExpiredWarningNotification();
            }
        }

        private static string QueryString(NavigationManager nav, string paramName)
        {
            var uri = nav.ToAbsoluteUri(nav.Uri);
            var paramValue = HttpUtility.ParseQueryString(uri.Query).Get(paramName);
            return paramValue ?? "";
        }

        public record NewBkAddEvent
        {
            [JsonPropertyName("title")] public string Title { get; set; }
            [JsonPropertyName("url")] public string Url { get; set; }
            [JsonPropertyName("tabId")] public int TabId { get; set; }
        }

        private async Task HandleNewBookmarkAddedEvent(object arg1)
        {
            var evt = JsonSerializer.Deserialize<NewBkAddEvent>(JsonSerializer.Serialize(arg1))!;
            Logger.LogInformation("Received : {Event}", evt);
            await BkEditFormData.LoadAsync(evt.Url, evt.Title, Array.Empty<string>());
            _modalVisible = true;
            _returnTabId = evt.TabId;
            StateHasChanged();
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
            await WebExtensions.Tabs.OpenAsync(StaticUrlOptions.Value.Docs);
        }

        private async Task OpenWhatsNew()
        {
            await WebExtensions.Tabs.OpenAsync(StaticUrlOptions.Value.WhatsNew);
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

        #region Notification

        private async Task AccessTokenExpiredWarningNotification()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (_userOptions?.PinyinFeature?.Enabled == true &&
                _userOptions.PinyinFeature?.ExpireDate.HasValue == true &&
                _userOptions.PinyinFeature.ExpireDate < DateTime.Now.AddDays(Consts.JwtExpiredWarningDays))
            {
                await NoticeWarning("PinyinAccessToken");
            }

            if (_userOptions?.CloudBkFeature?.Enabled == true &&
                _userOptions.CloudBkFeature?.ExpireDate.HasValue == true &&
                _userOptions.CloudBkFeature.ExpireDate < DateTime.Now.AddDays(Consts.JwtExpiredWarningDays))
            {
                await NoticeWarning("CloudBkAccessToken");
            }

            async Task NoticeWarning(string name)
            {
                await Notice.Open(new NotificationConfig()
                {
                    Message = $"{name} is about to expire",
                    Description = $"Your token will be expired within {Consts.JwtExpiredWarningDays} days, please try to create a new one.",
                    NotificationType = NotificationType.Warning
                });
            }
        }

        #endregion Notification

        public async ValueTask DisposeAsync()
        {
            await _moduleLoader.DisposeAsync();
        }
    }
}