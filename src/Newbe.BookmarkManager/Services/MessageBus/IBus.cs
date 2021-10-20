using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public interface IBus
    {
        Task EnsureStartAsync();
        void RegisterHandler(string messageType, RequestHandlerDelegate handler, string? messageId = null);
        Task SendMessage(BusMessage message);
    }
}