using Autofac;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public delegate bool RequestHandlerDelegate(ILifetimeScope scope, BusMessage message);
}