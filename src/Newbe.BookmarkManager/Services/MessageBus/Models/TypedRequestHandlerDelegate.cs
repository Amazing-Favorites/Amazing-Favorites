using Autofac;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public delegate void TypedRequestHandlerDelegate<in TMessage>(ILifetimeScope scope, TMessage message,
        BusMessage sourceMessage)
        where TMessage : IMessage;
}