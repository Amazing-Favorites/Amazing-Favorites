using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.Servers;

namespace Newbe.BookmarkManager.Services
{
    public class LPCServerJob : ILPCServerJob
    {

        private readonly ILPCServer _lpcServer;
        private readonly IBkSearcherServer _bkSearcherServer;
        private readonly IBkSearcher _bkSearcher;
        public LPCServerJob(ILPCServer lpcServer, IBkSearcherServer bkSearcherServer, IBkSearcher bkSearcher)
        {
            _lpcServer = lpcServer;
            _bkSearcherServer = bkSearcherServer;
            _bkSearcher = bkSearcher;
        }

        public async ValueTask StartAsync()
        {
            //_lpcServer.AddServerInstance(_bkSearcherServer);
            _lpcServer.AddHandler<BkSearchRequest, BkSearchResponse>(Search);
            await _lpcServer.StartAsync();
        }

        public async Task<BkSearchResponse> Search(BkSearchRequest request)
        {
            var result = await _bkSearcher.Search(request.SearchText, request.Limit);
            var response = new BkSearchResponse
            {
                ResultItems = result
            };
            return response;
        }
    }
}