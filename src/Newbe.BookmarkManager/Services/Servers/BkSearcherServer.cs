using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services.Servers
{
    public class BkSearcherServer : IBkSearcherServer
    {

        private readonly IBkSearcher _bkSearcher;

        public BkSearcherServer(IBkSearcher bkSearcher)
        {
            _bkSearcher = bkSearcher;
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

        public async Task<BkSearchResponse> History(BkSearchHistoryRequest request)
        {
            var result = await _bkSearcher.History(request.Limit);
            var response = new BkSearchResponse
            {
                ResultItems = result
            };
            return response;
        }
    }
}