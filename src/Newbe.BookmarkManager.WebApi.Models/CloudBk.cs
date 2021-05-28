using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.WebApi
{
    public record CloudBk
    {
        [JsonPropertyName("ts")] public List<string> Tags { get; init; } = new();
    }
}