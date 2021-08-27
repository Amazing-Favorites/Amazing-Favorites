using System;
using System.Collections.Generic;
using System.Text.Json;
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

        private readonly Dictionary<string, (Type eventType, Type handlerType, IAfEventHandler? instance)>
            _handlerTypeDict = new();

        public AfEventHub(
            ILogger<AfEventHub> logger,
            IRuntimeApi runtimeApi,
            ILifetimeScope lifetimeScope)
        {
            _logger = logger;
            _runtimeApi = runtimeApi;
            _lifetimeScope = lifetimeScope;
        }

        public async Task StartAsync()
        {
            await _runtimeApi.OnMessage.AddListener((o, sender, arg3) =>
            {
                Task.Run(async () =>
                {
                    var afEventEnvelope =
                        await JsonHelper.DeserializeAsync<AfEventEnvelope>(JsonSerializer.Serialize(o));
                    if (afEventEnvelope == null)
                    {
                        _logger.LogInformation("Not af event");
                        return;
                    }

                    if (string.IsNullOrEmpty(afEventEnvelope.TypeCode))
                    {
                        _logger.LogInformation("empty af event code");
                        return;
                    }

                    if (!_handlerTypeDict.TryGetValue(afEventEnvelope.TypeCode, out var registration))
                    {
                        _logger.LogDebug("registration not found");
                        return;
                    }

                    var (eventType, handlerType, instance) = registration;
                    IAfEventHandler handler;
                    if (instance == null)
                    {
                        if (!_lifetimeScope.TryResolve(handlerType, out var handlerObj))
                        {
                            _logger.LogError("failed to find handler from container for type: {Type}",
                                handlerType);
                            return;
                        }

                        if (handlerObj is not IAfEventHandler foundHandler)
                        {
                            _logger.LogError("handler type error: {HandlerObj}",
                                handlerObj);
                            return;
                        }

                        handler = foundHandler;
                    }
                    else
                    {
                        handler = instance;
                    }

                    var payload = await JsonHelper.DeserializeAsync(afEventEnvelope.PayloadJson, eventType);
                    _logger.LogInformation("afEventEnvelope.PayloadJson:{payload}",afEventEnvelope.PayloadJson);
                    _logger.LogInformation("payload:{payload}",payload);
                    if (payload == null)
                    {
                        _logger.LogError("failed to deserialize event payload: {Payload}",
                            payload);
                        return;
                    }

                    await handler.HandleAsync((IAfEvent)payload);
                    _logger.LogInformation("event handled success: {AfEvent}", afEventEnvelope);
                });
                return true;
            });
        }

        public void RegisterHandler<TEventType, THandlerType>()
            where TEventType : class, IAfEvent
            where THandlerType : class, IAfEventHandler
        {
            _handlerTypeDict[typeof(TEventType).Name] = (typeof(TEventType), typeof(THandlerType), default);
        }

        public void RegisterHandler<TEventType, THandlerType>(THandlerType eventHandler)
            where TEventType : class, IAfEvent where THandlerType : class, IAfEventHandler
        {
            _handlerTypeDict[typeof(TEventType).Name] = (typeof(TEventType), typeof(THandlerType), eventHandler);
        }

        public Task PublishAsync(IAfEvent afEvent)
        {
            var o = afEvent as object;
            var message = new AfEventEnvelope
            {
                TypeCode = afEvent.GetType().Name,
                PayloadJson = JsonSerializer.Serialize(o)
            };
#pragma warning disable 4014
            _runtimeApi.SendMessage("", message, new object());
#pragma warning restore 4014
            return Task.CompletedTask;
        }
    }
}