using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background
    {
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] public IUserOptionsService UserOptionsService { get; set; } = null!;
        [Inject] public IJobHost JobHost { get; set; }

        private UserOptions _userOptions = null!;

        private void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _userOptions = await UserOptionsService.GetOptionsAsync();
            await WebExtensions.Commands.OnCommand.AddListener(OnReceivedCommand);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await JobHost.StartAsync();
            }
        }
    }
}