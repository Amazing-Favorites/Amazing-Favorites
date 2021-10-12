using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduDriveClient
    {
        Task<string?> LoginAsync(bool interactive);
        Task<bool> TestAsync();

        Task<long?> UploadAsync(CloudBkCollection cloudBkCollection);
        Task<CloudBkCollection?> DownLoadFileByFileIdAsync();

        Task<long?> GetAfFieldId();
    }
}