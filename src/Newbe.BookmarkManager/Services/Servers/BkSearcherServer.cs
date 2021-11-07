using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.Servers
{
    public class BkSearcherServer : IBkSearcherServer
    {
        private readonly IBkSearcher _bkSearcher;

        public BkSearcherServer(IBkSearcher bkSearcher)
        {
            _bkSearcher = bkSearcher;
        }

        public async Task<BkSearchResponse> SearchAsync(BkSearchRequest request)
        {
            var result = await _bkSearcher.Search(request.SearchText, request.Limit);
            var response = new BkSearchResponse
            {
                ResultItems = result
            };
            return response;
        }

        public async Task<BkSearchResponse> GetHistoryAsync(BkSearchHistoryRequest request)
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