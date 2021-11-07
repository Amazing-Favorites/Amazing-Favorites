using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public class Bus : IBus
    {
        public delegate Bus Factory(BusOptions options);

        internal static readonly long DefaultExpiredDuration = (long)TimeSpan.FromSeconds(5).TotalSeconds;
        internal readonly MessageHandlerCollection MessageHandlerCollection;
        private readonly BusOptions _busOptions;
        private readonly ILogger<Bus> _logger;
        private readonly IStorageApiWrapper _storageApiWrapper;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IClock _clock;

        public Bus(
            BusOptions options,
            ILogger<Bus> logger,
            IStorageApiWrapper storageApiWrapper,
            ILifetimeScope lifetimeScope,
            IClock clock)
        {
            _busOptions = options;
            _logger = logger;
            _storageApiWrapper = storageApiWrapper;
            _lifetimeScope = lifetimeScope;
            _clock = clock;
            MessageHandlerCollection = new MessageHandlerCollection(_clock);
            BusId = RandomIdHelper.GetId();
        }

        private int _locker;

        public string BusId { get; }

        public async Task EnsureStartAsync()
        {
            if (Interlocked.Increment(ref _locker) != 1)
            {
                _logger.LogInformation("AfEventHub already started");
                return;
            }

            _logger.LogInformation("Start to run AfEventHub");

            await _storageApiWrapper.RegisterCallBack(OnReceivedChanged);
        }

        private void OnReceivedChanged(JsonElement jsonElement, string area)
        {
            _logger.LogDebug("receive storage changes {Changes}", jsonElement);
            if (jsonElement.TryGetProperty(_busOptions.EnvelopName, out var value))
            {
                if (value.TryGetProperty("newValue", out var newValue))
                {
                    var channelMessage = newValue.Deserialize<BusMessageEnvelop>();
                    if (channelMessage == null)
                    {
                        _logger.LogInformation("Not channel message");
                        return;
                    }

                    MessageHandlerCollection.Handle(channelMessage.Message, _lifetimeScope);
                }
            }
        }

        public void RegisterHandler(string messageType, RequestHandlerDelegate handler, string? messageId = null)
        {
            var eventName = messageType;
            if (string.IsNullOrEmpty(messageId))
            {
                MessageHandlerCollection.AddHandler(message => message.MessageType == messageType,
                    handler);
            }
            else
            {
                MessageHandlerCollection.AddHandler(
                    message => message.MessageType == messageType && message.ParentMessageId == messageId,
                    handler,
                    _clock.UtcNow + DefaultExpiredDuration);
            }
        }

        public async Task SendMessage(BusMessage message)
        {
            var messageType = message.MessageType;
            message.SenderId = BusId;
            _logger.LogInformation("Event published {MessageType}", messageType);
            try
            {
                var channelMessageEnvelop = new BusMessageEnvelop
                {
                    Message = message,
                    UtcNow = _clock.UtcNow
                };
                await _storageApiWrapper.SetLocal(new Dictionary<string, object>
                {
                    { _busOptions.EnvelopName, channelMessageEnvelop }
                });
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}