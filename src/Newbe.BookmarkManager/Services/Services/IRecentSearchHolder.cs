using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Services
{
    public interface IRecentSearchHolder
    {
        RecentSearch RecentSearch { get; }
        Task AddAsync(string text);
        Task<RecentSearch> LoadAsync();
    }
}