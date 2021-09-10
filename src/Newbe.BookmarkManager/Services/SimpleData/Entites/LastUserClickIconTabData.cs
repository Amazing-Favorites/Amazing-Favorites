namespace Newbe.BookmarkManager.Services.SimpleData
{
    public record LastUserClickIconTabData(int TabId, long ClickTime) : ISimpleData
    {
        public LastUserClickIconTabData() : this(0, 0)
        {
        }
    }
}