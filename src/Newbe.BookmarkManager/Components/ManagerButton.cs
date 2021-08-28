using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Components
{
    public partial class ManagerButton
    {
        [Inject] public ITabsApi Tabs { get; set; }
        [Inject] public IOptions<StaticUrlOptions> StaticUrlOptions { get; set; }
        [Inject] public IBkManager BkManager { get; set; }
        private bool _controlPanelVisible;

        private async Task OpenHelp()
        {
            await Tabs.OpenAsync(StaticUrlOptions.Value.Docs);
        }

        private async Task OpenWhatsNew()
        {
            await Tabs.OpenAsync(StaticUrlOptions.Value.WhatsNew);
        }

        private async Task OpenWelcome()
        {
            await Tabs.OpenAsync(StaticUrlOptions.Value.Welcome);
        }

        private Task OpenControlPanel()
        {
            _controlPanelVisible = true;
            return Task.CompletedTask;
        }

        private async Task OnClickResumeFactorySetting()
        {
            await BkManager.RestoreAsync();
            _controlPanelVisible = false;
        }
    }
}