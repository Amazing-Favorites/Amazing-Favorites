using System;
using System.Threading.Tasks;
using Autofac;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public interface IAfEventHub
    {
        string AfEventHubId { get; }
        Task EnsureStartAsync();

        void RegisterHandler<TEventType>(Action<ILifetimeScope, IAfEvent, BusMessage> action);

        Task PublishAsync(IAfEvent afEvent);
    }

    public static class AfEventHubExtensions
    {
        public static void RegisterHandler<TEventType>(this IAfEventHub afEventHub, Func<TEventType, Task> handler)
            where TEventType : class, IAfEvent
        {
            afEventHub.RegisterHandler<TEventType>((scope, e) => { handler.Invoke((TEventType)e); });
        }

        public static void RegisterHandler<TEventType>(this IAfEventHub afEventHub,
            Action<ILifetimeScope, IAfEvent> action)
            where TEventType : class, IAfEvent
        {
            afEventHub.RegisterHandler<TEventType>(
                (scope, e, sourceMessage) => { action.Invoke(scope, (TEventType)e); });
        }
    }
}