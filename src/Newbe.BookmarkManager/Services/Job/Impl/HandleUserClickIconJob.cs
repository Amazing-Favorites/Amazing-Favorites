using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.RPC;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Services
{
    public class HandleUserClickIconJob : IHandleUserClickIconJob
    {
        private readonly ILogger<HandleUserClickIconJob> _logger;
        private readonly IAfEventHub _afEventHub;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;
        private readonly IMediator _mediator;

        public HandleUserClickIconJob(
            ILogger<HandleUserClickIconJob> logger,
            IAfEventHub afEventHub,
            ISimpleDataStorage simpleDataStorage,
            IClock clock,
            IMediator mediator)
        {
            _logger = logger;
            _afEventHub = afEventHub;
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
            _mediator = mediator;
        }

        public async ValueTask StartAsync()
        {
            _afEventHub.RegisterHandler<UserClickAfIconEvent>(HandleUserClickAfIconEvent);
            await _mediator.EnsureStartAsync();
            await _afEventHub.EnsureStartAsync();
        }

        private async Task HandleUserClickAfIconEvent(UserClickAfIconEvent arg)
        {
            var data = new LastUserClickIconTabData(arg.TabId, _clock.UtcNow);
            _logger.LogInformation("user click icon: {Data}", data);
            await _simpleDataStorage.SaveAsync(data).ConfigureAwait(false);
        }
    }
}