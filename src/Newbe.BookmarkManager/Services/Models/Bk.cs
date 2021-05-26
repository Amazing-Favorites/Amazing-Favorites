using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record Bk
    {
        public string Title { get; set; }
        public Dictionary<TextAliasType, TextAlias> TitleAlias { get; set; }
        public string Url { get; init; }
        public string UrlHash { get; init; }
        public string FavIconUrl { get; set; }
        public List<string> Tags { get; set; } = new();
        public int ClickedCount { get; set; }
        public long LastClickTime { get; set; }
        public long TitleLastUpdateTime { get; set; }
        public long LastCreateTime { get; init; }
    }
}