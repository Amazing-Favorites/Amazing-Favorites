using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.WebApi
{
    [Serializable]
    public record CloudBkCollection
    {
        [JsonPropertyName("d")] public long LastUpdateTime { get; init; }

        [JsonPropertyName("e")] public long EtagVersion { get; set; }

        /// <summary>
        /// key: urlHash
        /// </summary>
        [JsonPropertyName("bks")]
        public Dictionary<string, CloudBk> Bks { get; init; } = new();
    }
}