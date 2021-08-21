using System;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public interface IAfEventHub
    {
        Task StartAsync();

        void RegisterHandler<TEventType, THandlerType>()
            where TEventType : class, IAfEvent
            where THandlerType : class, IAfEventHandler;

        void RegisterHandler<TEventType, THandlerType>(THandlerType eventHandler)
            where TEventType : class, IAfEvent
            where THandlerType : class, IAfEventHandler;

        Task PublishAsync(IAfEvent afEvent);
    }

    public static class AfEventHubExtensions
    {
        public static void RegisterHandler<TEventType>(this IAfEventHub afEventHub, Func<TEventType, Task> handler)
            where TEventType : class, IAfEvent
        {
            afEventHub.RegisterHandler<TEventType, FuncEventHandler>(new FuncEventHandler(e =>
                handler.Invoke((TEventType)e)));
        }
    }
}