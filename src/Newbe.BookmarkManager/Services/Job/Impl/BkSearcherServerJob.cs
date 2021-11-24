using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.MessageBus;
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

        private readonly INotificationRecordServer _notificationRecordServer;
        public BkSearcherServerJob(ILogger<BkSearcherServerJob> logger,
            ILPCServer lpcServer,
            IBkSearcherServer bkSearcherServer,
            IAfEventHub afEventHub,
            ISmallCache smallCache,
            INotificationRecordServer notificationRecordServer)
        {
            _logger = logger;
            _lpcServer = lpcServer;
            _bkSearcherServer = bkSearcherServer;
            _afEventHub = afEventHub;
            _smallCache = smallCache;
            _notificationRecordServer = notificationRecordServer;
        }

        public async ValueTask StartAsync()
        {
            var methodInfos = _lpcServer.AddServerInstance(_bkSearcherServer);
            _logger.LogInformation("There are {Count} method bind to LPCServer: {Names}",
                methodInfos.Count,
                methodInfos.Select(x => x.Name));
            var methodInfos2 = _lpcServer.AddServerInstance(_notificationRecordServer);
            _logger.LogInformation("There are {Count} method bind to LPCServer: {Names}",
                methodInfos2.Count,
                methodInfos2.Select(x => x.Name));

            await _lpcServer.StartAsync();
            
            

            _afEventHub.RegisterHandler<SmallCacheExpiredEvent>(Action);
            await _afEventHub.EnsureStartAsync();
        }

        private void Action(ILifetimeScope arg1, IAfEvent arg2, BusMessage sourceMessage)
        {
            if (arg2 is SmallCacheExpiredEvent evt &&
                sourceMessage.SenderId != _afEventHub.AfEventHubId)
            {
                _logger.LogInformation("cache expired and remove it");
                _smallCache.Remove(evt.CacheKey);
            }
        }
    }
}