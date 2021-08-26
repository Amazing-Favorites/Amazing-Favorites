using System;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.EventHubs.Events;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class UserOneDriveLoginSuccessHandler : EventHandlerBase<UserOneDriveLoginSuccessEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IClock _clock;
        public UserOneDriveLoginSuccessHandler(INotificationService notificationService, IClock clock)
        {
            _notificationService = notificationService;
            _clock = clock;
        }
        public override async Task HandleCoreAsync(UserOneDriveLoginSuccessEvent afEvent)
        {
            var entity = new NotificationRecord()
            {
                AfNotificationType = afEvent.AfNotificationType,
                Message = afEvent.Message,
                CreatedTime = DateTime.UtcNow
            };
            await _notificationService.AddAsync(entity);
        }
    }
}