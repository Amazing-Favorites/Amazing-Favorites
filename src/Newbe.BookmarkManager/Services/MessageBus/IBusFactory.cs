namespace Newbe.BookmarkManager.Services.MessageBus
{
    public interface IBusFactory
    {
        IBus Create(BusOptions options);
    }
}