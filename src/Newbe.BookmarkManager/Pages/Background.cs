using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public ISyncBookmarkJob SyncBookmarkJob { get; set; }
        [Inject] public ISyncAliasJob SyncAliasJob { get; set; }
        [Inject] public ISyncCloudJob SyncCloudJob { get; set; }
        [Inject] public ISyncTagRelatedBkCountJob SyncTagRelatedBkCountJob { get; set; }
        
        [JSInvokable]
        public void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await ImportAsync("content/background_keyboard.js");
            var lDotNetReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("GLOBAL.SetDotnetReference", lDotNetReference);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await SyncBookmarkJob.StartAsync();
                await SyncAliasJob.StartAsync();
                await SyncCloudJob.StartAsync();
                await SyncTagRelatedBkCountJob.StartAsync();
            }
        }
    }
}