using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduDriveClient
    {
        Task<string?> LoginAsync(bool interactive);
        Task<bool> TestAsync();

        Task<string> UploadAsync(CloudBkCollection cloudBkCollection);

    }
}
