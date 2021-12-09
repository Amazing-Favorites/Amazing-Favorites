using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services;

public record AfCode
{
    [JsonPropertyName("u")] public string? Url { get; set; }
    [JsonPropertyName("t")] public string? Title { get; set; }
    [JsonPropertyName("ts")] public string[]? Tags { get; set; }
}