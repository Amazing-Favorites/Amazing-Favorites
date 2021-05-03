using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ISyncAliasJob
    {
        ValueTask StartAsync();
    }
}