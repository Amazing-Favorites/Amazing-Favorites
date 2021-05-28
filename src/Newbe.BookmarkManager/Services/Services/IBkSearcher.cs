using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        Task InitAsync();
        SearchResultItem[] Search(string searchText, int limit);
    }
}