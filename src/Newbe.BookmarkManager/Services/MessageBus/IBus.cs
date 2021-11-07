using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public interface IBus
    {
        string BusId { get; }
        Task EnsureStartAsync();
        void RegisterHandler(string messageType, RequestHandlerDelegate handler, string? messageId = null);
        Task SendMessage(BusMessage message);
    }
}