using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly Dictionary<string, (Type eventType, Func<ILifetimeScope, MethodRequest,MethodResponse> handler)>
            _handlersDict = new();
        private readonly IClock _clock;
        private readonly ILogger<Mediator> _logger;
        private readonly IRuntimeApi _runtimeApi;
        private readonly ILifetimeScope _lifetimeScope;
        private const string AfIPCKey = "afIPCKey";
        private int _locker;
        private readonly ITabsApi _tabsApi;

        private Port _port;
        public Mediator(IClock clock, ILogger<Mediator> logger, ILifetimeScope lifetimeScope, IRuntimeApi runtimeApi)
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
        public async Task<bool> OnSendMessage(object message, MessageSender sender,Action<object> sendResponse)
        {
            var request = JsonSerializer.Deserialize<MethodRequest>(JsonSerializer.Serialize(message));
            if (request == null)
            {
                _logger.LogInformation("Not af request");
                return false;
            }

            var response = new MethodResponse
            {
                Id = request.Id,
                PayloadJson = request.PayloadJson
            };
            sendResponse(response);
            return true;
        }

        private async Task OnMessage()
        {
            await _runtimeApi.OnMessage.AddListener2((message, sender, callback) =>
            {

                _logger.LogInformation("OnMessage Installed");
                var request = JsonSerializer.Deserialize<MethodRequest>(JsonSerializer.Serialize(message));
                if (request == null)
                {
                    _logger.LogInformation("Not af request");
                    return false;
                }

                var array = request.PayloadJson.ToCharArray();
                Array.Reverse(array);
                var response = new MethodResponse
                {
                    Id = request.Id,
                    PayloadJson = new string(array)
                };
                _logger.LogInformation($"ID:{response.Id}_Payload: {response.PayloadJson}");
                callback(response);
                return true;
            });
            
        }
        public async Task<MethodResponse> Send(MethodRequest request)
        {
            _logger.LogInformation("Send Start");
            object? result = null;
            var t = JsonSerializer.Serialize(request);
            _logger.LogInformation(t);
            var sending =  await _runtimeApi.SendMessage(await _runtimeApi.GetId(), request,new object());
            var t2 = sending.EnumerateObject();
            foreach (var item in t2)
            {
                _logger.LogInformation($"Name:{item.Name},Value{item.Value}");

            }

            result = sending.Deserialize<MethodResponse>();
            if (result != null)
            {
                return JsonSerializer.Deserialize<MethodResponse>(JsonSerializer.Serialize(result));
            }

            throw new Exception("SendException");
        }
    }

}