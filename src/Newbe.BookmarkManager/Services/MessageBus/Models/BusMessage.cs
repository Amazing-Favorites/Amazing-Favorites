using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public record BusMessage
    {
        [JsonPropertyName("id")] public string? MessageId { get; set; }
        [JsonPropertyName("parentId")] public string? ParentMessageId { get; set; }
        [JsonPropertyName("messageType")] public string? MessageType { get; set; }
        [JsonPropertyName("payload")] public string? PayloadJson { get; set; }
        [JsonPropertyName("senderId")] public string SenderId { get; set; }
    }
}