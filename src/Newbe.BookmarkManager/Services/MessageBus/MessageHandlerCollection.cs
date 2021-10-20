using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ConcurrentCollections;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    internal class MessageHandlerCollection
    {
        private readonly IClock _clock;

        public MessageHandlerCollection(
            IClock clock)
        {
            _clock = clock;
        }

        internal readonly ConcurrentHashSet<HandlerItem> HandlerItems = new();

        public void AddHandler(Func<BusMessage, bool> filter, RequestHandlerDelegate handler,
            long expiredAt = default)
        {
            HandlerItems.Add(new HandlerItem
            {
                Filter = filter,
                Handler = handler,
                ExpiredAt = expiredAt
            });
        }

        public void Handle(BusMessage busMessage, ILifetimeScope lifetimeScope)
        {
            var toBeRemoved = new List<HandlerItem>(HandlerItems.Count);
            var filterSuccessList = new List<HandlerItem>(HandlerItems.Count);
            var now = _clock.UtcNow;
            foreach (var item in HandlerItems)
            {
                if (item.ExpiredAt != default)
                {
                    if (item.ExpiredAt < now)
                    {
                        toBeRemoved.Add(item);
                        continue;
                    }
                }

                if (item.Filter.Invoke(busMessage))
                {
                    filterSuccessList.Add(item);
                }
            }

            foreach (var item in filterSuccessList)
            {
                var done = item.Handler.Invoke(lifetimeScope, busMessage);
                if (done)
                {
                    toBeRemoved.Add(item);
                }
            }

            var items = HandlerItems
                .Where(x => x.Filter.Invoke(busMessage))
                .ToList();

            foreach (var item in toBeRemoved)
            {
                HandlerItems.TryRemove(item);
            }
        }

        internal record HandlerItem
        {
            public Func<BusMessage, bool> Filter { get; init; } = null!;
            public RequestHandlerDelegate Handler { get; init; } = null!;
            public long ExpiredAt { get; init; }
        }
    }
}