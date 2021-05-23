using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }

        [JSInvokable]
        public void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtension.Tabs.ActiveOrOpenManagerAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await ImportAsync("content/background_keyboard.js");
            var lDotNetReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("GLOBAL.SetDotnetReference", lDotNetReference);
        }
    }
}