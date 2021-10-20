using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserClickAfIconEvent : IAfEvent
    {
        [JsonPropertyName("tabId")] public int TabId { get; set; }
    }
}