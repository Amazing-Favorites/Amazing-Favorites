using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.MessageBus;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class AfEventHub : IAfEventHub
    {
        private readonly ILogger<AfEventHub> _logger;
        private readonly IBus _bus;

        private readonly Dictionary<string, (Type eventType, List<Action<ILifetimeScope, IAfEvent>> handler)>
            _handlersDict = new();

        public AfEventHub(
            ILogger<AfEventHub> logger,
            IBusFactory busFactory)
        {
            _logger = logger;
            _bus = busFactory.Create(new BusOptions
            {
                EnvelopName = Consts.BusEnvelopNames.AfEvent
            });
        }

        public string AfEventHubId => _bus.BusId;

        public async Task EnsureStartAsync()
        {
            await _bus.EnsureStartAsync();
        }

        public void RegisterHandler<TEventType>(Action<ILifetimeScope, IAfEvent, BusMessage> action)
        {
            _bus.RegisterHandler(action.Invoke, typeof(TEventType));
        }

        public async Task PublishAsync(IAfEvent afEvent)
        {
            await _bus.SendMessage(new BusMessage
            {
                MessageType = afEvent.GetType().Name,
                MessageId = RandomIdHelper.GetId(),
                PayloadJson = JsonSerializer.Serialize((object)afEvent),
            });
        }
    }
}