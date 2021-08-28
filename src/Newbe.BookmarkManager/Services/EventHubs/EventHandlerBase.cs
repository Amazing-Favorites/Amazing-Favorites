using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public abstract class EventHandlerBase<TEvent> : IAfEventHandler
    {
        public void Handle(IAfEvent afEvent)
        {
            HandleCoreAsync((TEvent)afEvent);
        }

        public abstract Task HandleCoreAsync(TEvent afEvent);
    }
}