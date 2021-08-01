using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background : IAsyncDisposable
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public ISyncBookmarkJob SyncBookmarkJob { get; set; }
        [Inject] public ISyncAliasJob SyncAliasJob { get; set; }
        [Inject] public ISyncCloudJob SyncCloudJob { get; set; }
        [Inject] public IDataFixJob DataFixJob { get; set; }
        [Inject] public ISyncTagRelatedBkCountJob SyncTagRelatedBkCountJob { get; set; }

        private IJSObjectReference? _keyboardEventModule;
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
                _keyboardEventModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", 
                    "/content/background_keyboard.js");
                var lDotNetReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("DotNet.SetDotnetReference", lDotNetReference);
                await DataFixJob.StartAsync();
                await SyncBookmarkJob.StartAsync();
                await SyncAliasJob.StartAsync();
                await SyncCloudJob.StartAsync();
                await SyncTagRelatedBkCountJob.StartAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_keyboardEventModule is not null)
            {
                await _keyboardEventModule.DisposeAsync();
            }
        }
    }
}