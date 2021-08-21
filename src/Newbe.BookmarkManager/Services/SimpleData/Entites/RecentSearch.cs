using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services.SimpleData
{
    public record RecentSearch : ISimpleData
    {
        public List<RecentSearchItem> Items { get; set; } = new();
    }

    public record RecentSearchItem
    {
        public string Text { get; set; }
        public long LastTime { get; set; }
        public int SearchCount { get; set; }
    }
}