using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public record BusMessageEnvelop
    {
        [JsonPropertyName("utcNow")] public long? UtcNow { get; set; } = null!;
        [JsonPropertyName("message")] public BusMessage Message { get; set; } = null!;
    }
}