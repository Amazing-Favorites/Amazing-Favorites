using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduDriveClient
    {
        Task<string?> LoginAsync(bool interactive);
        Task<bool> TestAsync();
        
    }
}
