namespace Newbe.BookmarkManager.Services.MessageBus
{
    public class BusFactory : IBusFactory
    {
        private readonly Bus.Factory _factory;

        public BusFactory(
            Bus.Factory factory)
        {
            _factory = factory;
        }

        public IBus Create(BusOptions options)
        {
            return _factory.Invoke(options);
        }
    }
}