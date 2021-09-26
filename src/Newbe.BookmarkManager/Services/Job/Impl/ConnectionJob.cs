using System;
using System.Threading.Tasks;
using WebExtensions.Net.Runtime;

namespace Newbe.BookmarkManager.Services
{
    public class ConnectionJob : IConnectionJob
    {

        private readonly IRuntimeApi _runtimeApi;
        public ConnectionJob(IRuntimeApi runtimeApi)
        {
            _runtimeApi = runtimeApi;
        }
        public async ValueTask StartAsync()
        {
            return;
        }
    }
}