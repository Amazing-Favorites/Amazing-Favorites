using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IGoogleDriveClient
    {
        Task<bool> LoginAsync(bool interactive);
        Task<CloudDataDescription?> GetFileDescriptionAsync();
        Task<CloudBkCollection?> GetCloudDataAsync();
        Task UploadAsync(CloudBkCollection cloudBkCollection);
        Task<bool> TestAsync();
    }

    public record GoogleDriveOAuthOptions
    {
        public GoogleDriveOAuthClientType Type { get; set; }
        public string ClientId { get; set; }
        public string DevClientId { get; set; }
        public string[] Scopes { get; set; }
    }

    public enum GoogleDriveOAuthClientType
    {
        Prod,
        Dev
    }

    public record CloudDataDescription
    {
        public long LastUpdateTime { get; set; }
        public long EtagVersion { get; set; }
    }
}