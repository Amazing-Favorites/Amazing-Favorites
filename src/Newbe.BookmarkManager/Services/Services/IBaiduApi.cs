


using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IBaiduApi
    {
        [Get("/api/quota")]
        Task<ApiResponse<BaiduQuotaResponse>> GetQuotaAsync(BaiduQuotaRequest baiduQuotaRequest);

        [Get("/rest/2.0/xpan/file?method=list")]
        Task<ApiResponse<BaiduFileListResponse>> GetFileListAsync(BaiduFileListRequest baiduFileListRequest);

        [Get("/rest/2.0/xpan/file?method=doclist")]
        Task<ApiResponse<BaiduDocListResponse>> GetDocListAsync(BaiduDocListRequest baiduDocListRequest);
        
        [Get("/rest/2.0/xpan/file?method=search")]
        Task<ApiResponse<BaiduSearchResponse>> SearchAsync(BaiduSearchRequest baiduSearchRequest);

        [Get("/rest/2.0/xpan/multimedia?method=filemetas")]
        Task<ApiResponse<BaiduFileMetasResponse>> GetFileMatesAsync(BaiduFileMetasRequest baiduFileMetasRequest);
    }

    public record BaiduQuotaRequest
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

    public record BaiduFileListRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        [AliasAs("dir")]
        public string? Dir { get; set; }
        [AliasAs("order")]
        public string? Order { get; set; }
        [AliasAs("desc")]
        public string? Desc { get; set; }
        [AliasAs("start")]
        public int? Start { get; set; }
        [AliasAs("limit")]
        public int? Limit { get; set; }
        [AliasAs("web")]
        public string? Web { get; set; }
        [AliasAs("folder")]
        public int? Folder { get; set; }
        [AliasAs("showempty")]
        public int? ShowEmpty { get; set; }
    }

    public record BaiduFileListResponse
    {
        [JsonPropertyName("errno")]
        public string Errno { get; set; }
        [JsonPropertyName("guid")]
        public long Guid { get; set; }
        [JsonPropertyName("guid_info")]
        public string GuidInfo { get; set; }
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
        [JsonPropertyName("list")]
        public BaiduFile[] List { get; set; }
        // [JsonPropertyName("fs_id")]
        // public long FsId { get; set; }
        // [JsonPropertyName("path")]
        // public string Path { get; set; }
        // [JsonPropertyName("server_filename")]
        // public string ServerFileName { get; set; }
        // [JsonPropertyName("size")]
        // public int Size { get; set; }
        // [JsonPropertyName("server_mtime")]
        // public int ServerMTime { get; set; }
        // [JsonPropertyName("server_ctime")]
        // public int ServerCTime { get; set; }
        // [JsonPropertyName("local_mtime")]
        // public int LocalMTime { get; set; }
        // [JsonPropertyName("local_ctime")]
        // public int LocalCTime { get; set; }
        // [JsonPropertyName("isdir")]
        // public int IsDir { get; set; }
        // [JsonPropertyName("category")]
        // public int Category { get; set; }
        // [JsonPropertyName("md5")]
        // public string MD5 { get; set; }
        // [JsonPropertyName("dir_empty")]
        // public int DirEmpty { get; set; }
        // [JsonPropertyName("thumbs")]
        // public string[] Thumbs { get; set; }
    }

    public record BaiduFile
    {
        [JsonProperty("category")]
        public int Category { get; set; }
        [JsonProperty("dir_empty")]
        public int DirEmpty { get; set; }
        [JsonProperty("empty")]
        public int Empty { get; set; }
        [JsonProperty("extent_tinyint7")]
        public int ExtentTinyInt7 { get; set; }
        [JsonProperty("fs_id")]
        public long FsId { get; set; }
        [JsonProperty("isdir")]
        public int IsDir { get; set; }
        [JsonProperty("local_mtime")]
        public long LocalMTime { get; set; }
        [JsonProperty("local_ctime")]
        public long LocalCTime { get; set; }
        [JsonProperty("oper_id")]
        public long OperId { get; set; }
        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }
        [JsonProperty("owner_type")]
        public int OwnerType { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("pl")]
        public int PL { get; set; }
        [JsonProperty("real_category")]
        public string realCategory { get; set; }
        [JsonProperty("server_atime")]
        public long ServerATime { get; set; }
        [JsonProperty("server_ctime")]
        public long ServerCTime { get; set; }
        
        [JsonProperty("server_filename")]
        public string ServerFileName { get; set; }
        [JsonProperty("server_mtime")]
        public long ServerMTime { get; set; }
        [JsonProperty("share")]
        public int Share { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("tkbind_id")]
        public long TKBindId { get; set; }
        [JsonProperty("unlist")]
        public int UnList { get; set; }
        [JsonProperty("wpfile")]
        public int WPFile { get; set; }
    }

    public record BaiduDocListRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        [AliasAs("page")]
        public int? Page { get; set; }
        [AliasAs("num")]
        public int? Num { get; set; }
        [AliasAs("order")]
        public string? Order { get; set; }
        [AliasAs("desc")]
        public string? Desc { get; set; }
        [AliasAs("parent_path")]
        public string? ParentPath { get; set; }
        [AliasAs("recursion")]
        public string? Recursion { get; set; }
        [AliasAs("web")]
        public int? Web { get; set; }
    }
    public record BaiduDocListResponse
    {
        
    }

    public record BaiduSearchRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        [AliasAs("key")]
        public string Key { get; set; }
        [AliasAs("dir")]
        public string Dir { get; set; }
        [AliasAs("page")]
        public int? Page { get; set; }
        [AliasAs("num")]
        public int? Num { get; set; }
        [AliasAs("recursion")]
        public string? Recursion { get; set; }
        [AliasAs("web")]
        public int? Web { get; set; }
    }
    public record BaiduSearchResponse
    {
        public dynamic ContentList { get; set; }
        [JsonProperty("errno")]
        public string Errno { get; set; }
        [JsonProperty("has_more")]
        public int HasMore { get; set; }
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("list")]
        public BaiduFile[] List { get; set; }
    }

    public record BaiduFileMetasRequest
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        
        [AliasAs("fsids")]
        public string FsIds { get; set; }
        
        [AliasAs("path")]
        public string? Path { get; set; }
        [AliasAs("thumb")]
        public int? Thumb { get; set; }
        [AliasAs("dlink")]
        public int? DLink { get; set; }
        [AliasAs("extra")]
        public int? Extra { get; set; }
    }

    public record BaiduFileMetasResponse
    {
        
    }
    
}