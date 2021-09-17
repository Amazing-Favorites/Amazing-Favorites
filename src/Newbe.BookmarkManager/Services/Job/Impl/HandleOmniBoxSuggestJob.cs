using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using WebExtensions.Net;
using WebExtensions.Net.Omnibox;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public class HandleOmniBoxSuggestJob : IHandleOmniBoxSuggestJob
    {
        private readonly ILogger<HandleOmniBoxSuggestJob> _logger;
        private readonly IAfEventHub _afEventHub;
        private readonly IBkSearcher _bkSearcher;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IWebExtensionsApi _webExtensions;
        public HandleOmniBoxSuggestJob(
            ILogger<HandleOmniBoxSuggestJob> logger,
            IAfEventHub afEventHub, IBkSearcher bkSearcher, IUserOptionsService userOptionsService, IWebExtensionsApi webExtensions)
        {
            _logger = logger;
            _afEventHub = afEventHub;
            _bkSearcher = bkSearcher;
            _userOptionsService = userOptionsService;
            _webExtensions = webExtensions;
        }

        public async ValueTask StartAsync()
        {
            _afEventHub.RegisterHandler<UserOptionSaveEvent>(HandleUserOptionSaveEvent);
            await _afEventHub.EnsureStartAsync();
        }

        private async Task HandleUserOptionSaveEvent(UserOptionSaveEvent arg)
        {
            switch (arg.OminiboxSuggestChanged)
            {
                case true when arg?.UserOptions?.OmniboxSuggestFeature?.Enabled == true:
                    _logger.LogInformation("addOmniBoxSuggest");
                    await AddOmniBoxSuggestAsync();
                    break;
                case true when arg?.UserOptions?.OmniboxSuggestFeature?.Enabled == false:
                    _logger.LogInformation(" removeOmniBoxSuggest");
                    await RemoveOmniBoxSuggestAsync();
                    break;
            }
        }
        #region OmniBox
        private async Task<SuggestResult[]> GetOmniBoxSuggest(string input)
        {
            var option = (await _userOptionsService.GetOptionsAsync())?.OmniboxSuggestFeature;
            if (option == null || option.Enabled == false)
            {
                return Array.Empty<SuggestResult>();
            }
            var searchResult = await _bkSearcher.Search(input, option.SuggestCount);
            var suggestResults = searchResult.Select(a => new SuggestResult
            {
                Content = a.Bk.Url,
                Description = a.Bk.Title
            }).ToArray();

            return suggestResults;

        }
        private async Task AddOmniBoxSuggestAsync()
        {
            await _webExtensions.Omnibox.OnInputChanged.AddListener(OmniboxSuggestActiveAsync);
            await _webExtensions.Omnibox.OnInputEntered.AddListener(OmniboxSuggestTabOpenAsync);
        }

        private async Task RemoveOmniBoxSuggestAsync()
        {
            await _webExtensions.Omnibox.OnInputChanged.RemoveListener(OmniboxSuggestActiveAsync);
            await _webExtensions.Omnibox.OnInputEntered.RemoveListener(OmniboxSuggestTabOpenAsync);
        }

        private async void OmniboxSuggestActiveAsync(string input, Action<IEnumerable<SuggestResult>> suggest)
        {
            var result = await GetOmniBoxSuggest(input);
            suggest(result);
        }
        private async void OmniboxSuggestTabOpenAsync(string url, OnInputEnteredDisposition disposition)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                var managerTabTitle = Consts.AppName;
                var managerTabs = await _webExtensions.Tabs.Query(new QueryInfo { Title = managerTabTitle });
                if (managerTabs.Any())
                {
                    await _webExtensions.Tabs.Update(managerTabs.FirstOrDefault().Id, new UpdateProperties { Active = true });
                }
                else
                {
                    await _webExtensions.Tabs.Create(new CreateProperties { Url = "/Manager/index.html" });
                }

                return;
            }

            switch (disposition)
            {
                case OnInputEnteredDisposition.CurrentTab:
                    await _webExtensions.Tabs.Update(tabId: null, updateProperties: new UpdateProperties { Url = url });
                    break;
                case OnInputEnteredDisposition.NewForegroundTab:
                    await _webExtensions.Tabs.Create(new CreateProperties { Url = url, Active = true });
                    break;
                case OnInputEnteredDisposition.NewBackgroundTab:
                    await _webExtensions.Tabs.Create(new CreateProperties { Url = url, Active = false });
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}