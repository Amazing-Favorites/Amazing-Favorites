using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.EventHubs;
using WebExtensions.Net.Omnibox;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background : IAsyncDisposable
    {
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] public IUserOptionsService UserOptionsService { get; set; } = null!;
        [Inject] public IJobHost JobHost { get; set; }
        
        [Inject] public IBkSearcher BkSearcher { get; set; }
        
        [Inject]
        public IAfEventHub AfEventHub { get; set; }

        private JsModuleLoader _moduleLoader = null!;

        [JSInvokable]
        public void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                _moduleLoader = new JsModuleLoader(JsRuntime);
                await _moduleLoader.LoadAsync("/content/background_keyboard.js");
                var userOptions = await UserOptionsService.GetOptionsAsync();
                if (userOptions is
                    {
                        AcceptPrivacyAgreement: true,
                        ApplicationInsightFeature:
                        {
                            Enabled: true
                        }
                    })
                {
                    await _moduleLoader.LoadAsync("/content/ai.js");
                }

                if (userOptions?.OmniboxSuggestFeature?.Enabled == true)
                {
                    await AddOnimiBoxSuggestAsync();
                }
                AfEventHub.RegisterHandler<UserOptionSaveEvent>(HandleUserOptionSaveEvent);
                var lDotNetReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("DotNet.SetDotnetReference", lDotNetReference);
                await JobHost.StartAsync();
                
            }
        }
        
        private Task HandleUserOptionSaveEvent(UserOptionSaveEvent evt)
        {
            if (evt.OminiboxSuggestChanged == false)
            {
                return Task.CompletedTask;
            }
            
            if (evt?.UserOptions?.OmniboxSuggestFeature?.Enabled == true)
            {
                return InvokeAsync(async () =>
                {
                    Console.WriteLine("addOnimiBox");
                    await AddOnimiBoxSuggestAsync();
                });
            }

            return InvokeAsync(async () =>
            {
                Console.WriteLine("removeOnimiBox");
                await RemoveOnimiBoxSuggestAsync();
            });
        }
        public async Task<SuggestResult[]> GetOnimiBoxSuggest(string input)
        {
            var option = (await UserOptionsService.GetOptionsAsync())?.OmniboxSuggestFeature;
            if (option == null || option.Enabled == false)
            {
                return Array.Empty<SuggestResult>();
            }
            var searchResult = await BkSearcher.Search(input, option.SuggestCount);
            var suggestResults = searchResult.Select(a => new SuggestResult
            {
                Content = a.Bk.Url,
                Description = a.Bk.Title
            }).ToArray();

            return suggestResults;

        }
        public async Task AddOnimiBoxSuggestAsync()
        {
            await WebExtensions.Omnibox.OnInputChanged.AddListener(OmniboxSuggestAction);
            await WebExtensions.Omnibox.OnInputEntered.AddListener(OmniboxSuggestCallback);
        }

        public async Task RemoveOnimiBoxSuggestAsync()
        {
            await WebExtensions.Omnibox.OnInputChanged.RemoveListener(OmniboxSuggestAction);
            await WebExtensions.Omnibox.OnInputEntered.RemoveListener(OmniboxSuggestCallback);
        }
        
        async void OmniboxSuggestAction(string input, Action<IEnumerable<SuggestResult>> suggest)
        {
            var result = await GetOnimiBoxSuggest(input);
            suggest(result);
        }
        async void OmniboxSuggestCallback(string url, OnInputEnteredDisposition disposition)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                var managerTabTitle = "Amazing Favorites";
                var managerTabs = await WebExtensions.Tabs.Query(new QueryInfo {Title = managerTabTitle});
                if (managerTabs.Any())
                {
                    await WebExtensions.Tabs.Update(managerTabs.FirstOrDefault().Id, new UpdateProperties {Active = true});
                }
                else
                {
                    await WebExtensions.Tabs.Create(new CreateProperties {Url = "/Manager/index.html"});
                }

                return;
            }

            switch (disposition)
            {
                case OnInputEnteredDisposition.CurrentTab:
                    await WebExtensions.Tabs.Update(tabId: null, updateProperties: new UpdateProperties {Url = url});
                    break;
                case OnInputEnteredDisposition.NewForegroundTab:
                    await WebExtensions.Tabs.Create(new CreateProperties {Url = url, Active = true});
                    break;
                case OnInputEnteredDisposition.NewBackgroundTab:
                    await WebExtensions.Tabs.Create(new CreateProperties {Url = url, Active = false});
                    break;
                default:
                    break;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _moduleLoader.DisposeAsync();
        }
    }
}