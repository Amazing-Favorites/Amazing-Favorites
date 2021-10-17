using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.LPC
{
    public interface ILPCClient<out T>
    {
        Task StartAsync();
        Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request)
            where TResponse : IResponse
            where TRequest : IRequest;
    }
}