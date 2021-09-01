using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using WebExtensions.Net.Runtime;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class AfEventHub : IAfEventHub
    {
        private readonly ILogger<AfEventHub> _logger;
        private readonly IRuntimeApi _runtimeApi;
        private readonly ILifetimeScope _lifetimeScope;

        private readonly Dictionary<string, (Type eventType, List<Action<ILifetimeScope, IAfEvent>> handler)>
            _handlersDict = new();

        public AfEventHub(
            ILogger<AfEventHub> logger,
            IRuntimeApi runtimeApi,
            ILifetimeScope lifetimeScope)
        {
            _logger = logger;
            _runtimeApi = runtimeApi;
            _lifetimeScope = lifetimeScope;
        }

        private int _locker;

        public async Task EnsureStartAsync()
        {
            if (Interlocked.Increment(ref _locker) != 1)
            {
                _logger.LogInformation("AfEventHub already started");
                return;
            }

            _logger.LogInformation("Start to run AfEventHub");

            await _runtimeApi.OnMessage.AddListener(OnReceivedMessage);
        }

        private bool OnReceivedMessage(object o, MessageSender sender, Action arg3)
        {
            var afEventEnvelope = JsonSerializer.Deserialize<AfEventEnvelope>(JsonSerializer.Serialize(o));
            if (afEventEnvelope == null)
            {
                _logger.LogInformation("Not af event");
                return false;
            }

            var typeCode = afEventEnvelope.TypeCode;
            if (string.IsNullOrEmpty(typeCode))
            {
                _logger.LogInformation("empty af event code");
                return false;
            }

            if (!_handlersDict.TryGetValue(typeCode, out var registration))
            {
                _logger.LogInformation("registration not found for {TypeCode}", typeCode);
                return false;
            }

            var (eventType, handlers) = registration;
            var payload = JsonSerializer.Deserialize(afEventEnvelope.PayloadJson, eventType);
            if (payload == null)
            {
                _logger.LogError("failed to deserialize event payload: {Payload}", payload);
                return false;
            }

            foreach (var handler in handlers)
            {
                handler.Invoke(_lifetimeScope, (IAfEvent)payload);
            }


            _logger.LogInformation("event handled success: {AfEvent}",
                afEventEnvelope with { PayloadJson = string.Empty });
            return true;
        }

        public void RegisterHandler<TEventType>(Action<ILifetimeScope, IAfEvent> action)
        {
            var eventName = typeof(TEventType).Name;
            List<Action<ILifetimeScope, IAfEvent>> handlers;
            if (!_handlersDict.TryGetValue(eventName, out var registration))
            {
                handlers = new List<Action<ILifetimeScope, IAfEvent>>();
                registration = (typeof(TEventType), handlers);
            }
            else
            {
                handlers = registration.handler;
            }

            handlers.Add(action);
            _handlersDict[eventName] = registration;
        }

        public Task PublishAsync(IAfEvent afEvent)
        {
            var typeCode = afEvent.GetType().Name;
            var message = new AfEventEnvelope
            {
                TypeCode = typeCode,
                PayloadJson = JsonSerializer.Serialize((object)afEvent)
            };
            _logger.LogInformation("Event published {TypeCode}", typeCode);
            // current page can not receive runtime message
            OnReceivedMessage(message, default!, default!);
            try
            {
                _runtimeApi.SendMessage("", message, new object());
            }
            catch (Exception ex)
            {
                // ignore
            }
            return Task.CompletedTask;
        }
    }
}