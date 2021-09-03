using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background : IAsyncDisposable
    {
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] public IUserOptionsService UserOptionsService { get; set; } = null!;
        [Inject] public IJobHost JobHost { get; set; }
        
        [Inject] public IBkSearcher BkSearcher { get; set; }

        private JsModuleLoader _moduleLoader = null!;

        [JSInvokable]
        public void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
            }
        }
        [JSInvokable]
        public async Task<SuggestResult[]> GetOnimiBoxSuggest(string input)
        {
            var searchResult = await BkSearcher.Search(input, 3);
            var t1 = searchResult.Select(a => new SuggestResult
            {
                Content = a.Bk.Url,
                Description = a.Bk.Title
            }).ToArray();

            return t1;

        }
        [JSInvokable]
        public async Task<bool> CheckIsUrl(string url)
        {
           return Uri.TryCreate(url, UriKind.Absolute,out _);

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

                if (userOptions is
                {
                    OmniboxSuggestFeature:
                    {
                        Enabled: true
                    }
                })
                {
                    await _moduleLoader.LoadAsync("/content/omnibox_suggest.js");
                }

                var lDotNetReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("DotNet.SetDotnetReference", lDotNetReference);
                await JobHost.StartAsync();
                
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _moduleLoader.DisposeAsync();
        }
    }


    public record SuggestResult
    {
        public string Content { get; set; }

        public string Description { get; set; }
    }
}