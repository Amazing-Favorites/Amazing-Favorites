using AntDesign;

namespace Newbe.BookmarkManager.Services
{
    public record BkViewItem(Bk Bk)
    {
        public bool ShowIndex { get; set; }
        public int LineIndex { get; set; }
        public string NewTag { get; set; }
        public bool NewTagInputVisible { get; set; }
        public Input<string> NewTagRef { get; set; }
    }
}