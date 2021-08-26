﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.Services.EventHubs.Events;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class UserOneDriveLoginSuccessHandler : EventHandlerBase<UserOneDriveLoginSuccessEvent>
    {
        private readonly INotificationRecordService _notificationService;
        private readonly IClock _clock;
        private readonly ILogger<UserOneDriveLoginSuccessHandler> _logger;
        public UserOneDriveLoginSuccessHandler(INotificationRecordService notificationService, IClock clock, ILogger<UserOneDriveLoginSuccessHandler> logger)
        {
            _notificationService = notificationService;
            _clock = clock;
            _logger = logger;
        }
        public override async Task HandleCoreAsync(UserOneDriveLoginSuccessEvent afEvent)
        {
            var entity = new NotificationRecord()
            {
                AfNotificationType = afEvent.AfNotificationType,
                Message = afEvent.Message,
                CreatedTime = DateTime.UtcNow
            };
            _logger.LogDebug("UserOneDriveLoginSuccessEvent:" + entity);
            await _notificationService.AddAsync(entity);
        }
    }
}