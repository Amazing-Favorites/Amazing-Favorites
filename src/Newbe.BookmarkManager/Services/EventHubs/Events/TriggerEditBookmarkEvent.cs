using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record TriggerEditBookmarkEvent : IAfEvent
    {
        [JsonPropertyName("title")] public string Title { get; set; } = null!;
        [JsonPropertyName("url")] public string Url { get; set; } = null!;
        [JsonPropertyName("tabId")] public int TabId { get; set; }
    }
}