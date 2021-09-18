using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.RPC
{
    public interface ISender
    {
        Task<MethodResponse> MethodCall(MethodRequest methodRequest);
    }
}