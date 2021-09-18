using System;
using System.Text.Json.Serialization;

namespace Newbe.BookmarkManager.Services.RPC
{
    public record MethodResponse
    {
        [JsonPropertyName("Id")] public Guid Id { get; set; }
        
        [JsonPropertyName("typeCode")] public string TypeCode { get; set; }
        
        [JsonPropertyName("payloadJson")] public string PayloadJson { get; set; }
    }
}