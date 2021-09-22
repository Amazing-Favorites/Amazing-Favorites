using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.RPC
{
    public interface ISender
    {
        public  Task<MethodResponse> Send(MethodRequest request);
    }
}