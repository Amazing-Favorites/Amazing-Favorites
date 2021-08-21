using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserNotificationEvent : IAfEvent
    {
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("afNotificationType")]
        public AfNotificationType AfNotificationType { get; set; }
    }

    public enum AfNotificationType
    {
        Info,
        Warning
    }
}