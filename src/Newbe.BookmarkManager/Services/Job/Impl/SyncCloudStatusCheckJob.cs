using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Services
{
    public class SyncCloudStatusCheckJob : ISyncCloudStatusCheckJob
    {
        private readonly IAfEventHub _afEventHub;
        private readonly ILogger<SyncCloudStatusCheckJob> _logger;
        private readonly IGoogleDriveClient _googleDriveClient;
        private readonly IOneDriveClient _oneDriveClient;
        private readonly INotificationRecordService _notificationRecord;

        public SyncCloudStatusCheckJob(
            IAfEventHub afEventHub,
            ILogger<SyncCloudStatusCheckJob> logger,
            IGoogleDriveClient googleDriveClient,
            IOneDriveClient oneDriveClient,
            INotificationRecordService notificationRecord)
        {
            _afEventHub = afEventHub;
            _logger = logger;
            _googleDriveClient = googleDriveClient;
            _oneDriveClient = oneDriveClient;
            _notificationRecord = notificationRecord;
        }

        public async ValueTask StartAsync()
        {
            _afEventHub.RegisterHandler<UserLoginSuccessEvent>(HandleUserLoginSuccessEvent);
            await _afEventHub.EnsureStartAsync();
        }

        private async Task HandleUserLoginSuccessEvent(UserLoginSuccessEvent afEvent)
        {
            _logger.LogInformation("received {Event}", nameof(UserLoginSuccessEvent));
            var (cloudBkProviderType, accessToken) = afEvent;
            switch (cloudBkProviderType)
            {
                case CloudBkProviderType.NewbeApi:
                    break;
                case CloudBkProviderType.GoogleDrive:
                    _googleDriveClient.LoadToken(accessToken);
                    await TriggerSync();
                    break;
                case CloudBkProviderType.OneDrive:
                    _oneDriveClient.LoadToken(accessToken);
                    await TriggerSync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await SaveAsync(cloudBkProviderType);
            await _afEventHub.PublishAsync(new UserNotificationEvent()
            {
                AfNotificationType = AfNotificationType.Info,
                Message = $"{Enum.GetName(typeof(CloudBkProviderType), cloudBkProviderType)} login message",
                Description = "Login Succeed",
            });
            Task TriggerSync()
            {
                return _afEventHub.PublishAsync(new TriggerCloudSyncEvent());
            }

            Task SaveAsync(CloudBkProviderType type)
            {
                var entity = new NotificationRecord()
                {
                    AfNotificationType = AfNotificationType.Info,
                    Message = $"{Enum.GetName(typeof(CloudBkProviderType), type)} login message",
                    Description = "Login Succeed",
                    CreatedTime = DateTime.UtcNow
                };
                
                return _notificationRecord.AddAsync(entity);
            }
        }
    }
}