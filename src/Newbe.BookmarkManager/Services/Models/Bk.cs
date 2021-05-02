using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record Bk
    {
        public string Title { get; set; }
        public Dictionary<BkAliasType, string> TitleAlias { get; } = new();
        public string Url { get; init; }
        public string FavIconUrl { get; set; }
        public List<string> Tags { get; set; } = new();
        public int ClickedCount { get; set; }
        public long LastClickTime { get; set; }
        public long LastCreateTime { get; set; }
    }
}