using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IRecentSearchHolder
    {
        RecentSearch RecentSearch { get; }
        Task AddAsync(string text);
        Task<RecentSearch> LoadAsync();
    }
}