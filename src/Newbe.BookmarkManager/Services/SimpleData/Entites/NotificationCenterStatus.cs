namespace Newbe.BookmarkManager.Services.SimpleData
{
    public record NotificationCenterStatus : ISimpleData
    {
        public bool NewMessage { get; set; }
    }
}