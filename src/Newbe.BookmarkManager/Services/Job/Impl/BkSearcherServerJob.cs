using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.Servers;

namespace Newbe.BookmarkManager.Services
{
    public class BkSearcherServerJob : IBkSearcherServerJob
    {
        private readonly ILogger<BkSearcherServerJob> _logger;
        private readonly ILPCServer _lpcServer;
        private readonly IBkSearcherServer _bkSearcherServer;
        private readonly IAfEventHub _afEventHub;
        private readonly ISmallCache _smallCache;

        public BkSearcherServerJob(ILogger<BkSearcherServerJob> logger,
            ILPCServer lpcServer,
            IBkSearcherServer bkSearcherServer,
            IAfEventHub afEventHub,
            ISmallCache smallCache)
        {
            _logger = logger;
            _lpcServer = lpcServer;
            _bkSearcherServer = bkSearcherServer;
            _afEventHub = afEventHub;
            _smallCache = smallCache;
        }

        public async ValueTask StartAsync()
        {
            var methodInfos = _lpcServer.AddServerInstance(_bkSearcherServer);
            _logger.LogInformation("There are {Count} method bind to LPCServer: {Names}",
                methodInfos.Count,
                methodInfos.Select(x => x.Name));
            await _lpcServer.StartAsync();

            _afEventHub.RegisterHandler<SmallCacheExpiredEvent>(HandleSmallCacheExpiredEvent);
            await _afEventHub.EnsureStartAsync();
        }

        private Task HandleSmallCacheExpiredEvent(SmallCacheExpiredEvent evt)
        {
            _logger.LogInformation("cache expired and remove it");
            _smallCache.Remove(evt.CacheKey);
            return Task.CompletedTask;
        }
    }
}