using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.WebApi
{
    public record SaveToCloudOutput
    {
        [JsonPropertyName("o")] public bool IsOk { get; set; }
        [JsonPropertyName("m")] public string Message { get; set; }
    }
}