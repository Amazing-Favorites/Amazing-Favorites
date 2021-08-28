using System;
using System.Threading.Tasks;
using Autofac;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public interface IAfEventHub
    {
        Task EnsureStartAsync();

        void RegisterHandler<TEventType>(Action<ILifetimeScope, IAfEvent> action);

        Task PublishAsync(IAfEvent afEvent);
    }

    public static class AfEventHubExtensions
    {
        public static void RegisterHandler<TEventType>(this IAfEventHub afEventHub, Func<TEventType, Task> handler)
            where TEventType : class, IAfEvent
        {
            afEventHub.RegisterHandler<TEventType>((scope, e) => { handler.Invoke((TEventType)e); });
        }
    }
}