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
using WebExtensions.Net.Storage;
using JsonSerializer = System.Text.Json.JsonSerializer;
namespace Newbe.BookmarkManager.Services.RPC
{
    public class Mediator:IMediator
    {
        private readonly Dictionary<string, (Type eventType, Func<ILifetimeScope, MethodRequest,MethodResponse> handler)>
            _handlersDict = new();
        private readonly IClock _clock;
        private readonly ILogger<Mediator> _logger;
        private readonly IStorageApi _storageApi;
        private readonly ILifetimeScope _lifetimeScope;
        private const string AfIPCKey = "afIPCKey";
        private int _locker;
        public Mediator(IClock clock, IStorageApi storageApi, ILogger<Mediator> logger, ILifetimeScope lifetimeScope)
        {
            _clock = clock;
            _storageApi = storageApi;
            _logger = logger;
            _lifetimeScope = lifetimeScope;
        }
        
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new ArgumentNullException(nameof(request));
        }

        public async Task EnsureStartAsync()
        {
            if (Interlocked.Increment(ref _locker) != 1)
            {
                _logger.LogInformation("AfEventHub already started");
                return;
            }

            _logger.LogInformation("Start to run AfEventHub");

            await _storageApi.OnChanged.AddListener(OnReceivedRequestChanged);
            await _storageApi.OnChanged.AddListener(OnReceivedResponseChanged);
        }
        private void OnReceivedRequestChanged(object changes, string area)
        {
            var jsonElement = (JsonElement)changes;
            if (jsonElement.TryGetProperty(AfIPCKey, out var value))
            {
                if (value.TryGetProperty("newValue", out var newValue))
                {
                    if (newValue.TryGetProperty("request", out var request))
                    {
                        OnRequestMessage(request);
                    }
                }
            }
        }
        private void OnReceivedResponseChanged(object changes, string area)
        {
            var jsonElement = (JsonElement)changes;
            if (jsonElement.TryGetProperty(AfIPCKey, out var value))
            {
                if (value.TryGetProperty("newValue", out var newValue))
                {
                    if (newValue.TryGetProperty("response", out var response))
                    {
                        OnResponseMessage(response);
                    }
                }
            }
        }
        public async Task<MethodResponse> MethodCall(MethodRequest methodRequest)
        {
            methodRequest.Id = Guid.NewGuid();
            //TaskCompletionSource<MethodResponse> methodCallCompletionSource = new TaskCompletionSource<MethodResponse>();
            if (_handlersDict.TryAdd(methodRequest.TypeCode, methodCallCompletionSource))
            {
                //await _hubContext.Clients.User(userId).MethodCall(methodParams);

                
                
                
                return await methodCallCompletionSource.Task;
            }

            throw new Exception("Couldn't call the method.");
        }

        private async Task<bool> OnRequestMessage(object o)
        {
            var request = JsonSerializer.Deserialize<MethodRequest>(JsonSerializer.Serialize(o));
            if (request == null)
            {
                return false;
            }

            var typeCode = request.TypeCode;
            if (string.IsNullOrEmpty(typeCode))
            {
                return false;
            }
            
            if (!_handlersDict.TryGetValue(typeCode, out var registration))
            {
                _logger.LogInformation("registration not found for {TypeCode}", typeCode);
                return false;
            }
            var (eventType, handler) = registration;
            var payloadJson = request.PayloadJson;
            _logger.LogDebug("deserialize to {Type} from JSON {Json}", eventType, payloadJson);
            var payload = JsonConvert.DeserializeObject(payloadJson, eventType);
            var response = handler.Invoke(_lifetimeScope, (MethodRequest)payload);
            
            try
            {
                var local = await _storageApi.GetLocal();
                await local.Set(new
                {
                    afEvent = new
                    {
                        utcNow = _clock.UtcNow,
                        response
                    }
                });
            }
            catch (Exception)
            {
                // ignore
            }

            return true;

        }

        private bool OnResponseMessage(object o)
        {
            var response = JsonSerializer.Deserialize<MethodResponse>(JsonSerializer.Serialize(o));
            if (response == null)
            {
                return false;
            }

            var typeCode = response.TypeCode;
            if (string.IsNullOrEmpty(typeCode))
            {
                return false;
            }
            
        }
    }
}