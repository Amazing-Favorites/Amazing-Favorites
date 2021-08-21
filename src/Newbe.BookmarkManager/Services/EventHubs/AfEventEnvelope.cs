using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record AfEventEnvelope
    {
        [JsonPropertyName("typeCode")] public string TypeCode { get; set; }
        [JsonPropertyName("payloadJson")] public string PayloadJson { get; set; }
    }
}