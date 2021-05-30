using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        Task<SearchResultItem[]> Search(string searchText, int limit);
    }
}