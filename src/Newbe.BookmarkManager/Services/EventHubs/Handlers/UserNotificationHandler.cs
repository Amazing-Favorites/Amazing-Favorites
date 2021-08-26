using System;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class UserNotificationHandler : EventHandlerBase<UserNotificationEvent>
    {
        private readonly INotificationRecordService _notificationService;
        private readonly IClock _clock;

        public UserNotificationHandler(INotificationRecordService notificationService, IClock clock)
        {
            _notificationService = notificationService;
            _clock = clock;
        }
        public override async Task HandleCoreAsync(UserNotificationEvent afEvent)
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