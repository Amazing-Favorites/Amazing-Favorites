using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;
using Newtonsoft.Json;
using WebExtensions.Net.Runtime;
using WebExtensions.Net.Storage;
using WebExtensions.Net.Tabs;
using ConnectInfo = WebExtensions.Net.Runtime.ConnectInfo;
using JsonSerializer = System.Text.Json.JsonSerializer;
namespace Newbe.BookmarkManager.Services.RPC
{
    public class Mediator:IMediator
    {
        private readonly Dictionary<string, (Type eventType, Func<ILifetimeScope, IRequest, object> handler)>
            _handlersDict = new();
        private readonly IClock _clock;
        private readonly ILogger<Mediator> _logger;
        private readonly IRuntimeApi _runtimeApi;
        private readonly ILifetimeScope _lifetimeScope;
        private int _locker;
        private readonly ITabsApi _tabsApi;

        private Port _port;
        public Mediator(IClock clock, ILogger<Mediator> logger, ILifetimeScope lifetimeScope, IRuntimeApi runtimeApi )
        {
            _clock = clock;
            _logger = logger;
            _lifetimeScope = lifetimeScope;
            _runtimeApi = runtimeApi;
        }
        
        public async Task EnsureStartAsync()
        {
            if (Interlocked.Increment(ref _locker) != 1)
            {
                _logger.LogInformation("AfEventHub already started");
                return;
            }
            _logger.LogInformation("Start to run Mediator");
            await OnMessage();
        }
        // private async Task OnMessage()
        // {
        //     await _runtimeApi.OnMessage.AddListener2((message, sender, callback) =>
        //     {
        //
        //         _logger.LogInformation("OnMessage Installed");
        //         var request = JsonSerializer.Deserialize<MethodRequest>(JsonSerializer.Serialize(message));
        //         if (request == null)
        //         {
        //             _logger.LogInformation("Not af request");
        //             return false;
        //         }
        //
        //         var array = request.PayloadJson.ToCharArray();
        //         Array.Reverse(array);
        //         var response = new MethodResponse
        //         {
        //             Id = request.Id,
        //             PayloadJson = new string(array)
        //         };
        //         _logger.LogInformation($"ID:{response.Id}_Payload: {response.PayloadJson}");
        //         callback(response);
        //         return true;
        //     });
        //     
        // }
        private async Task OnMessage()
        {
            await _runtimeApi.OnMessage.AddListener2((message, sender, callback) => 
            {
                _logger.LogInformation("OnMessage hitted");
                var envelope = JsonSerializer.Deserialize<AfEventEnvelope>(JsonSerializer.Serialize(message));
                if (envelope == null)
                {
                    _logger.LogInformation("Not af request");
                    return false;
                }

                var typeCode = envelope.TypeCode;
                if (string.IsNullOrEmpty(typeCode))
                {
                    _logger.LogInformation("empty af request code");
                    return false;
                }
                if (!_handlersDict.TryGetValue(typeCode, out var registration))
                {
                    _logger.LogInformation("registration not found for {TypeCode}", typeCode);
                    return false;
                }
                var (eventType, handler) = registration;
                var payloadJson = envelope.PayloadJson;
                var payload = JsonConvert.DeserializeObject(payloadJson, eventType);
                if (payload == null)
                {
                    _logger.LogError("failed to deserialize event payload: {Payload}", payload);
                    return false;
                }

                var response =  handler.Invoke(_lifetimeScope, (IRequest) payload);
                
                
                _logger.LogInformation($"Response:{JsonConvert.SerializeObject(response)}");
                
                
                callback(response);
                return true;
            });
            
        }
        public async Task<TResponse> Send<TResponse>(IRequest request)
        {
            var typeCode = request.GetType().Name;
            var envelope = new AfEventEnvelope
            {
                TypeCode = typeCode,
                PayloadJson = JsonSerializer.Serialize((object) request)
            };
            var sending =  await _runtimeApi.SendMessage(await _runtimeApi.GetId(), envelope,new object());

            foreach (var item in sending.EnumerateObject())
            {
                _logger.LogInformation($"Name:{item.Name},Value:{item.Value}");
            }

            // if (sending.TryGetProperty("result", out var tmpValue))
            // {
            //     
            // }
            // _logger.LogInformation($"Target:{tmpValue.}");
            var result = JsonSerializer
                .Deserialize<TResponse>(JsonSerializer.Serialize((object)sending.EnumerateObject().FirstOrDefault(a=>a.Name == "result").Value));
            if (result != null)
            {
                return result;
            }
            throw new Exception("SendException");
        }

        public void RegisterHandler<TRequest>(Func<ILifetimeScope, IRequest, object> func)
        {
            var requestName = typeof(TRequest).Name;
            Func<ILifetimeScope, IRequest, object> handler;
            if (!_handlersDict.TryGetValue(requestName, out var registration))
            {
                registration = (typeof(TRequest), func);
                _handlersDict[requestName] = registration;
            }
            
        }
    }

}