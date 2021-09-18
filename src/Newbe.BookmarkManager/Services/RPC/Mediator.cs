using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebExtensions.Net.Storage;

namespace Newbe.BookmarkManager.Services.RPC
{
    public class Mediator:IMediator
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<MethodResponse>> _pendingMethodCalls = new ConcurrentDictionary<Guid, TaskCompletionSource<MethodResponse>>();
        private readonly IClock _clock;
        
        private readonly IStorageApi _storageApi;
        
        public Mediator(IClock clock, IStorageApi storageApi)
        {
            _clock = clock;
            _storageApi = storageApi;
        }
        
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new ArgumentNullException(nameof(request));
        }

        public async Task<MethodResponse> MethodCall(MethodRequest methodRequest)
        {
            methodRequest.Id = Guid.NewGuid();
            TaskCompletionSource<MethodResponse> methodCallCompletionSource = new TaskCompletionSource<MethodResponse>();
            if (_pendingMethodCalls.TryAdd(methodRequest.Id, methodCallCompletionSource))
            {
                //await _hubContext.Clients.User(userId).MethodCall(methodParams);

                
                
                
                return await methodCallCompletionSource.Task;
            }

            throw new Exception("Couldn't call the method.");
        }

        private bool OnRequestMessage(object o)
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