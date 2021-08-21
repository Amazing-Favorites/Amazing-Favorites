using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public interface IAfEventHandler
    {
        Task HandleAsync(IAfEvent afEvent);
    }
}