using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.LPC
{
    public class LPCServer : ILPCServer
    {
        private readonly IBus _bus;
        private readonly ILogger<LPCServer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public LPCServer(
            ILogger<LPCServer> logger,
            IBusFactory busFactory,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _bus = busFactory.Create(new BusOptions
            {
                EnvelopName = Consts.BusEnvelopNames.LPCServer
            });
        }

        public ILPCServer AddHandler<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            return AddHandler<TRequest, TResponse>(request => Task.FromResult(handler.Invoke(request)));
        }

        public ILPCServer AddHandler<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            _logger.LogInformation("New handler added, request: {RequestType}, response: {ResponseType}",
                typeof(TRequest),
                typeof(TResponse));
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