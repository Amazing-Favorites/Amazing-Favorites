using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduDriveClient
    {
        void LoadToken(string token);
        Task<string?> LoginAsync(bool interactive);
        Task<bool> TestAsync();

        Task<long?> UploadAsync(CloudBkCollection cloudBkCollection);
        Task<CloudBkCollection?> DownLoadFileByFileIdAsync();

        Task<long?> GetAfFieldId();
    }
    public record BaiduDriveOAuthOptions
    {
        public OAuth2ClientType Type { get; set; }
        public string ClientId { get; set; }
        public string DevClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}