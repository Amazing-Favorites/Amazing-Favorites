namespace Newbe.BookmarkManager.Services.SimpleData
{
    public record GoogleDriveStatics : ISimpleData
    {
        public long? LastSuccessUploadTime { get; set; }
    }
}