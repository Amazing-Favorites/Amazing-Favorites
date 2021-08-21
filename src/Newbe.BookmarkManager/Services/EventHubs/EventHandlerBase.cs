using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public abstract class EventHandlerBase<TEvent> : IAfEventHandler
    {
        public Task HandleAsync(IAfEvent afEvent)
        {
            return HandleCoreAsync((TEvent)afEvent);
        }

        public abstract Task HandleCoreAsync(TEvent afEvent);
    }
}