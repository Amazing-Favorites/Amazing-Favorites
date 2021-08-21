using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class GoogleBackgroundLoginResultEvent : IAfEvent
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
    }
}