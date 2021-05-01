using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ISyncBookmarkJob
    {
        ValueTask StartAsync();
        ValueTask LoadNowAsync();
    }
}