using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using WebExtensions.Net;
using WebExtensions.Net.Tabs;
namespace Newbe.BookmarkManager.Services
{
    public class HandleUserLastClickJob:IHandleUserLastClickJob
    {
        private readonly IAfEventHub _afEventHub;
        private readonly IWebExtensionsApi _webExtensions;
        private readonly IBkManager _bkManager;

        public HandleUserLastClickJob(
            ILogger<HandleUserClickIconJob> logger,
            IAfEventHub afEventHub,
            IClock clock, IWebExtensionsApi webExtensions, IBkManager bkManager)
        {
            _afEventHub = afEventHub;
            _webExtensions = webExtensions;
            _bkManager = bkManager;
        }
        
        public async ValueTask StartAsync()
        {
            await _webExtensions.Tabs.OnUpdated.AddListener(async (tabId,changeInfo,tab) =>
            {
                await _bkManager.AddClickAsync(changeInfo.Url, 1);
                await _afEventHub.PublishAsync(new RefreshManagerPageEvent());
            });
        }
        
        
    }
    
}