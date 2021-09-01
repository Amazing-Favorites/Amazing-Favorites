using System.Text.Json;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class NewNotification : INewNotification
    {
        private readonly INotificationRecordService _notificationRecordService;

        public NewNotification(
            INotificationRecordService notificationRecordService)
        {
            _notificationRecordService = notificationRecordService;
        }

        public Task NewReleaseAsync(NewReleaseInput input)
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.NewRelease,
                Title = "🆕 New version released!",
                Message = "A new version of Amazing Favorites released.",
                ArgsJson = JsonSerializer.Serialize(input),
            });
        }

        public Task WelcomeAsync()
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.Welcome,
                Title = "🌟 Welcome! My friend!",
                Message =
                    "Thank you very much for installing this extension and we hope we can help you in the coming days. You can learn the basic usage of this extension by following the link, just have fun!",
            });
        }

        public Task PrivacyAgreementUpdateAsync()
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.PrivacyAgreementUpdated,
                Title = "⚖ Privacy Agreement updated",
                Message =
                    "User Privacy Agreement has been updated recently. please review the agreement before you use some cloud-related feature"
            });
        }

        public Task PinyinTokenExpiredAsync(PinyinTokenExpiredInput input)
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.PinyinTokenExpired,
                Title = "⏲ Your token is about to expired",
                Message =
                    $"Token for Pinyin feature is about to expired within {input.LeftDays} days, please renew one.",
            });
        }

        public Task CloudBkTokenExpiredAsync(CloudBkTokenExpiredInput input)
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.CloudBkTokenExpired,
                Title = "⏲ Your token is about to expired",
                Message =
                    $"Token for Cloud data sync feature is about to expired within {input.LeftDays} days, please renew one.",
            });
        }

        public Task SuccessToSyncBkWithCloudAsync(SuccessToSyncBkWithCloudInput input)
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.SuccessToSyncBkWithCloud,
                Title = "🌤 Sync with cloud success",
                Message =
                    "Your data is sync with cloud success",
                ArgsJson = JsonSerializer.Serialize(input)
            });
        }

        public Task SyncDataWithCloudAsync(SyncDataWithCloudInput input)
        {
            return _notificationRecordService.AddAsync(new MsgItem
            {
                Type = UserNotificationType.SyncDataWithCloud,
                Title = "☁ Remind to sync to cloud",
                Message =
                    "You haven`t sync your data for a few time, please click the button to keep your data up to date",
                ArgsJson = JsonSerializer.Serialize(input)
            });
        }
    }
}