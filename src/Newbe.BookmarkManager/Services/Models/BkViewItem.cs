namespace Newbe.BookmarkManager.Services
{
    public record BkViewItem(Bk Bk)
    {
        public bool ShowIndex { get; set; }
        public int LineIndex { get; set; }
    }
}