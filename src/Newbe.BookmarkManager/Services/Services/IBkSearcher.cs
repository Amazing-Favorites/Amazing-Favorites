using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using static Newbe.BookmarkManager.Services.Ai.Events;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        [Insight(EventName = BkSearchEvent)]
        Task<SearchResultItem[]> Search(string searchText, int limit);
    }
}