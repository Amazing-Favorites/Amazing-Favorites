using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface INewNotification
    {
        Task NewReleaseAsync(NewReleaseInput input);
        Task WelcomeAsync();
        Task PrivacyAgreementUpdateAsync();
        Task PinyinTokenExpiredAsync(PinyinTokenExpiredInput input);
        Task CloudBkTokenExpiredAsync(CloudBkTokenExpiredInput input);
        Task SuccessToSyncBkWithCloudAsync(SuccessToSyncBkWithCloudInput input);
        Task SyncDataWithCloudAsync(SyncDataWithCloudInput input);
    }

    public record NewReleaseInput
    {
        public string Version { get; set; } = null!;
        public string WhatsNewUrl { get; set; } = null!;
    }

    public record PinyinTokenExpiredInput
    {
        public int LeftDays { get; set; }
    }

    public record CloudBkTokenExpiredInput
    {
        public int LeftDays { get; set; }
    }

    public record SuccessToSyncBkWithCloudInput
    {
        public CloudBkProviderType CloudBkProviderType { get; set; }
    }

    public record SyncDataWithCloudInput
    {
        public long LastSyncTime { get; set; }
    }
}