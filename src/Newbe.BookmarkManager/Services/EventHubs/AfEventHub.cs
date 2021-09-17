using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebExtensions.Net.Storage;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class AfEventHub : IAfEventHub
    {
        private readonly ILogger<AfEventHub> _logger;
        private readonly IStorageApi _storageApi;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;

        private readonly Dictionary<string, (Type eventType, List<Action<ILifetimeScope, IAfEvent>> handler)>
            _handlersDict = new();

        public AfEventHub(
            ILogger<AfEventHub> logger,
            IStorageApi storageApi,
            ILifetimeScope lifetimeScope,
            IClock clock)
        {
            _logger = logger;
            _storageApi = storageApi;
            _lifetimeScope = lifetimeScope;
            _clock = clock;
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

            await _storageApi.OnChanged.AddListener(OnReceivedChanged);
        }

        private const string AfEventKey = "afEvent";

        private void OnReceivedChanged(object changes, string area)
        {
            var jsonElement = (JsonElement)changes;
            if (jsonElement.TryGetProperty(AfEventKey, out var value))
            {
                if (value.TryGetProperty("newValue", out var newValue))
                {
                    if (newValue.TryGetProperty("message", out var message))
                    {
                        OnReceivedMessage(message);
                    }
                }
            }
        }

        private bool OnReceivedMessage(object o)
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
            var payloadJson = afEventEnvelope.PayloadJson;
            _logger.LogDebug("deserialize to {Type} from JSON {Json}", eventType, payloadJson);
            var payload = JsonConvert.DeserializeObject(payloadJson, eventType);
            // ??? its strangely, it doesn't work below
            // var payload = JsonHelper.Deserialize(payloadJson, eventType);
            if (payload == null)
            {
                _logger.LogError("failed to deserialize event payload: {Payload}", payload);
                return false;
            }

            foreach (var handler in handlers)
            {
                handler.Invoke(_lifetimeScope, (IAfEvent)payload);
            }
#if DEBUG
            afEventEnvelope = afEventEnvelope with { PayloadJson = string.Empty };
#endif
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

        public async Task PublishAsync(IAfEvent afEvent)
        {
            var typeCode = afEvent.GetType().Name;
            var message = new AfEventEnvelope
            {
                TypeCode = typeCode,
                PayloadJson = JsonSerializer.Serialize((object)afEvent)
            };
            _logger.LogInformation("Event published {TypeCode}", typeCode);
            try
            {
                var local = await _storageApi.GetLocal();
                await local.Set(new
                {
                    afEvent = new
                    {
                        utcNow = _clock.UtcNow,
                        message
                    }
                });
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}