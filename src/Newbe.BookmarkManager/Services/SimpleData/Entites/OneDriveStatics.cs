namespace Newbe.BookmarkManager.Services.SimpleData
{
    public record OneDriveStatics : ISimpleData
    {
        public long? LastSuccessUploadTime { get; set; }
    }
}