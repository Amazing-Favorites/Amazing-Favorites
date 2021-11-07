using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.Servers;
using WebExtensions.Net;
using WebExtensions.Net.Omnibox;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public class HandleOmniBoxSuggestJob : IHandleOmniBoxSuggestJob
    {
        private readonly ILogger<HandleOmniBoxSuggestJob> _logger;
        private readonly IAfEventHub _afEventHub;
        private readonly ILPCClient<IBkSearcherServer> _lpcClient;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IWebExtensionsApi _webExtensions;
        private readonly Subject<SearchItem> _subject = new();
        private UserOptions _userOptions = null!;

        public HandleOmniBoxSuggestJob(
            ILogger<HandleOmniBoxSuggestJob> logger,
            IAfEventHub afEventHub,
            IBkSearcher bkSearcher,
            IUserOptionsService userOptionsService,
            IWebExtensionsApi webExtensions,
            ILPCClient<IBkSearcherServer> lpcClient)
        {
            _logger = logger;
            _afEventHub = afEventHub;
            _userOptionsService = userOptionsService;
            _webExtensions = webExtensions;
            _lpcClient = lpcClient;
        }

        public async ValueTask StartAsync()
        {
            await _lpcClient.StartAsync();
            _afEventHub.RegisterHandler<UserOptionSaveEvent>(HandleUserOptionSaveEvent);
            await _afEventHub.EnsureStartAsync();
            _subject.Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(HandleSearch);
            _userOptions = await _userOptionsService.GetOptionsAsync();
            if (_userOptions.OmniboxSuggestFeature?.Enabled == true)
            {
                await AddOmniBoxSuggestAsync();
            }
        }

        private int _searchSequence = 0;

        private void HandleSearch(SearchItem item)
        {
            var input = item.Input;
            Task.Run(async () =>
            {
                // always search in the latest sequence
                if (_searchSequence != item.SearchSeqId)
                {
                    return;
                }

                var result = await GetOmniBoxSuggest(input);
                item.CallBack(result);
            });
        }

        private async Task HandleUserOptionSaveEvent(UserOptionSaveEvent arg)
        {
            var enableStateChange = _userOptions.OmniboxSuggestFeature?.Enabled !=
                                    arg.UserOptions.OmniboxSuggestFeature?.Enabled;
            _userOptions = arg.UserOptions;
            if (enableStateChange)
            {
                if (_userOptions.OmniboxSuggestFeature?.Enabled == true)
                {
                    await AddOmniBoxSuggestAsync();
                }
                else
                {
                    await RemoveOmniBoxSuggestAsync();
                }
            }
        }

        #region OmniBox

        private async Task<SuggestResult[]> GetOmniBoxSuggest(string input)
        {
            var option = (await _userOptionsService.GetOptionsAsync())?.OmniboxSuggestFeature;
            if (option?.Enabled != true)
            {
                return Array.Empty<SuggestResult>();
            }

            var searchResponse = await _lpcClient.InvokeAsync<BkSearchRequest, BkSearchResponse>(new BkSearchRequest
            {
                SearchText = input,
                Limit = option.SuggestCount
            });

            var suggestResults = searchResponse.ResultItems
                .Select(a => new SuggestResult
                {
                    Content = a.Bk.Url,
                    Description = new SuggestResultDescriptionBuilder()
                        .AddText(a.Bk.Title)
                        .AddUrl(a.Bk.Url)
                        .AddDim(string.Join(", ", a.Bk.Tags?.ToArray() ?? Array.Empty<string>()))
                        .Build(),
                }).ToArray();
            return suggestResults;
        }

        private async Task AddOmniBoxSuggestAsync()
        {
            _logger.LogInformation("addOmniBoxSuggest");
            await _webExtensions.Omnibox.OnInputChanged.AddListener(OmniboxSuggestActiveAsync);
            await _webExtensions.Omnibox.OnInputEntered.AddListener(OmniboxSuggestTabOpenAsync);
        }

        private async Task RemoveOmniBoxSuggestAsync()
        {
            _logger.LogInformation(" removeOmniBoxSuggest");
            await _webExtensions.Omnibox.OnInputChanged.RemoveListener(OmniboxSuggestActiveAsync);
            await _webExtensions.Omnibox.OnInputEntered.RemoveListener(OmniboxSuggestTabOpenAsync);
        }

        private async void OmniboxSuggestActiveAsync(string input, Action<IEnumerable<SuggestResult>> suggest)
        {
            _subject.OnNext(new SearchItem
            {
                Input = input,
                CallBack = suggest,
                SearchSeqId = Interlocked.Increment(ref _searchSequence)
            });
        }

        private async void OmniboxSuggestTabOpenAsync(string url, OnInputEnteredDisposition disposition)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                await _webExtensions.Tabs.ActiveOrOpenManagerAsync();
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
            }
        }

        #endregion

        private record SearchItem
        {
            public string Input { get; set; }
            public Action<IEnumerable<SuggestResult>> CallBack { get; set; }
            public int SearchSeqId { get; set; }
        }
    }

    public class SuggestResultDescriptionBuilder
    {
        private readonly StringBuilder _sb = new();

        private static string EscapeXmlChars(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            return source.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        public SuggestResultDescriptionBuilder AddUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _sb.Append($"<url>{EscapeXmlChars(url)}</url> ");
            }

            return this;
        }

        public SuggestResultDescriptionBuilder AddMatch(string match)
        {
            if (!string.IsNullOrEmpty(match))
            {
                _sb.Append($"<match>{EscapeXmlChars(match)}</match> ");
            }

            return this;
        }

        public SuggestResultDescriptionBuilder AddDim(string dim)
        {
            if (!string.IsNullOrEmpty(dim))
            {
                _sb.Append($"<dim>{EscapeXmlChars(dim)}</dim> ");
            }

            return this;
        }

        public SuggestResultDescriptionBuilder AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _sb.Append(EscapeXmlChars(text));
            }

            return this;
        }

        public string Build()
        {
            return _sb.ToString();
        }
    }
}