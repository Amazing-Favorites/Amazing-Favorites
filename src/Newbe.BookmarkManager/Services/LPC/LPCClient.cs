using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.LPC
{
    public class LPCClient<T> : ILPCClient<T>
    {
        private readonly IBus _bus;

        public LPCClient(
            IBusFactory busFactory)
        {
            _bus = busFactory.Create(new BusOptions
            {
                EnvelopName = Consts.BusEnvelopNames.LPCServer
            });
        }

        public async Task StartAsync()
        {
            await _bus.EnsureStartAsync();
        }

        public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
            where TResponse : IResponse
            where TRequest : IRequest
        {
            var response = await _bus.SendRequest<TRequest, TResponse>(request);
            return response;
        }
    }
}