using System;
using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserOneDriveLoginSuccessEvent : UserNotificationEvent
    {
    }
}