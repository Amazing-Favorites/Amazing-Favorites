using System;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.LPC
{
    public class LPCServer : ILPCServer
    {
        private readonly IBus _bus;

        public LPCServer(
            IBusFactory busFactory)
        {
            _bus = busFactory.Create(new BusOptions
            {
                EnvelopName = Consts.BusEnvelopNames.LPCServer
            });
        }

        public ILPCServer AddHandler<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            _bus.RegisterHandler<TRequest>((scope, message, sourceMessage) =>
            {
                var response = handler.Invoke(message);
#pragma warning disable 4014
                _bus.SendResponse(response, sourceMessage);
#pragma warning restore 4014
            });
            return this;
        }

        public async Task StartAsync()
        {
            await _bus.EnsureStartAsync();
        }
    }
}