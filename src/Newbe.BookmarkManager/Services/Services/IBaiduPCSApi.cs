

using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduPCSApi
    {
        [Multipart]
        [Post("/rest/2.0/pcs/superfile2?method=upload")]
        Task<UploadResponse> UploadAsync([Query]UploadRequest uploadRequest,[AliasAs("myPhoto")] StreamPart stream);
    }

    public record UploadRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }

        [AliasAs("method")]
        public string Method { get; set; }

        [AliasAs("type")]
        public string Type { get; set; }
        
        [AliasAs("path")]
        public string Path { get; set; }

        [AliasAs("uploadid")]
        public string Uploadid { get; set; }
        [AliasAs("partseq")]
        public int PartSeq { get; set; }
    }
    public record UploadBody
    {
        [AliasAs("file")] public char[] File { get; set; }
    }

    public record UploadResponse
    {
        [JsonPropertyName("md5")]
        public string Md5 { get; set; }
        [JsonPropertyName("uploadid")]
        public string UploadId { get; set; }
        [JsonPropertyName("partseq")]
        public string PartSeq { get; set; }
        [JsonPropertyName("request_id")]
        public long RequestId { get; set; }
    }
}