using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.WebApi
{
    public record GetCloudOutput
    {
        [JsonPropertyName("t")] public long LastUpdateTime { get; set; }
        [JsonPropertyName("e")] public long EtagVersion { get; set; }
        [JsonPropertyName("bks")] public CloudBkCollection? CloudBkCollection { get; set; }
    }
}