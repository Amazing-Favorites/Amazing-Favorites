using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public interface IAfEventHandler
    {
        void Handle(IAfEvent afEvent);
    }
}