


using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduApi
    {
        [Get("/api/quota")]
        Task<ApiResponse<BaiduQuotaResponse>> GetQuotaAsync(BaiduQuotaRequest baiduQuotaRequest);
    }

    public class BaiduQuotaRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        [AliasAs("checkfree")]
        public int CheckFree { get; set; }
        [AliasAs("checkexpire")]
        public int CheckExpire { get; set; }
    }
    public record BaiduQuotaResponse
    {
        [JsonPropertyName("errno")]
        public string Errno { get; set; }
        [JsonPropertyName("total")]
        public string Total { get; set; }
        [JsonPropertyName("expire")]
        public bool Expire { get; set; }
        [JsonPropertyName("used")]
        public string Used { get; set; }
        [JsonPropertyName("free")]
        public string Free { get; set; }
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }
    
}