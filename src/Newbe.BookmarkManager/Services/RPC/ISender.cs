using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.RPC
{
    public interface ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest request);
    }
}