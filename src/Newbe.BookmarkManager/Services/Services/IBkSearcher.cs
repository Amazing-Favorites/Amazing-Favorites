using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        [Insight(EventName = "Bk Search Event")]
        Task<SearchResultItem[]> Search(string searchText, int limit);
    }
}