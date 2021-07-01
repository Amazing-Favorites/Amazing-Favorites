using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.Bks)]
    public record Bk : IEntity<string>
    {
        public string Id => Url;
        public string Title { get; set; }
        public Dictionary<TextAliasType, TextAlias> TitleAlias { get; set; }
        public string Url { get; init; }
        public string UrlHash { get; set; }
        public string FavIconUrl { get; set; }
        public List<string>? Tags { get; set; } = new();
        public int ClickedCount { get; set; }
        public long LastClickTime { get; set; }
        public long TitleLastUpdateTime { get; set; }
        public long LastCreateTime { get; init; }
    }
}