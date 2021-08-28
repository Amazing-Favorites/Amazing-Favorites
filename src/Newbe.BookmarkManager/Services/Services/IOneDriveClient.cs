using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IOneDriveClient
    {
        void LoadToken(string token);
        Task<string?> LoginAsync(bool interactive);
        Task<CloudDataDescription?> GetFileDescriptionAsync();
        Task<CloudBkCollection?> GetCloudDataAsync();
        Task UploadAsync(CloudBkCollection cloudBkCollection);
        Task<bool> TestAsync();
    }
}