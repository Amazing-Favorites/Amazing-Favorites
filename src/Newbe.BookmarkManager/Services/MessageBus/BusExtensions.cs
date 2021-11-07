using System;
using System.Text.Json;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public static class BusExtensions
    {
        public static void RegisterHandler<TMessage>(this IBus dispatcher,
            TypedRequestHandlerDelegate<TMessage> handlerDelegate,
            Type messageType)
            where TMessage : IMessage
        {
            var requestTypeName = messageType.Name;
            dispatcher.RegisterHandler(requestTypeName, (scope, message) =>
            {
                HandleCore();
                return false;

                void HandleCore()
                {
                    var payloadJson = message.PayloadJson;
                    if (string.IsNullOrEmpty(payloadJson))
                    {
                        return;
                    }

                    var payload = (TMessage)JsonSerializer.Deserialize(payloadJson, messageType)!;
                    handlerDelegate.Invoke(scope, payload, message);
                }
            });
        }

        public static void RegisterHandler<TMessage>(this IBus dispatcher,
            TypedRequestHandlerDelegate<TMessage> handlerDelegate)
            where TMessage : IMessage
            => dispatcher.RegisterHandler(handlerDelegate, typeof(TMessage));

        public static void RegisterHandler(this IBus dispatcher,
            TypedRequestHandlerDelegate<IAfEvent> handlerDelegate,
            Type afEventType)
            => dispatcher.RegisterHandler<IAfEvent>(handlerDelegate, afEventType);

        public static async Task SendResponse<TResponse>(this IBus dispatcher, TResponse response,
            BusMessage requestMessage)
            where TResponse : IResponse
        {
            await SendResponse(dispatcher, Task.FromResult(response), requestMessage);
        }

        public static async Task SendResponse<TResponse>(this IBus dispatcher, Task<TResponse> responseTask,
            BusMessage requestMessage)
            where TResponse : IResponse
        {
            var resp = await responseTask;
            var parentId = requestMessage.MessageId;
            var requestTypeName = typeof(TResponse).Name;
            var channelMessage = new BusMessage
            {
                MessageId = RandomIdHelper.GetId(),
                MessageType = requestTypeName,
                PayloadJson = JsonSerializer.Serialize((object)resp!),
                ParentMessageId = parentId
            };
            await dispatcher.SendMessage(channelMessage);
        }

        public static async Task<TResponse> SendRequest<TRequest, TResponse>(this IBus dispatcher,
            TRequest request)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            var requestTypeName = typeof(TRequest).Name;
            var responseTypeName = typeof(TResponse).Name;

            var channelMessage = new BusMessage
            {
                MessageId = RandomIdHelper.GetId(),
                MessageType = requestTypeName,
                PayloadJson = JsonSerializer.Serialize((object)request)
            };
            var tcs = new TaskCompletionSource<TResponse>();
            dispatcher.RegisterHandler(responseTypeName, (scope, responseMessage) =>
            {
                if (responseMessage.ParentMessageId == channelMessage.MessageId)
                {
                    var payloadJson = responseMessage.PayloadJson;
                    if (string.IsNullOrEmpty(payloadJson))
                    {
                        return false;
                    }

                    var result = (TResponse)JsonSerializer.Deserialize(payloadJson, typeof(TResponse))!;
                    tcs.TrySetResult(result);
                    return true;
                }

                return false;
            }, channelMessage.MessageId);
            await dispatcher.SendMessage(channelMessage);
            var delay = Task.Delay(TimeSpan.FromSeconds(Bus.DefaultExpiredDuration));
            var resultTask = await Task.WhenAny(delay, tcs.Task);
            if (resultTask == delay)
            {
                tcs.TrySetException(new TimeoutException());
            }

            return tcs.Task.Result;
        }
    }
}