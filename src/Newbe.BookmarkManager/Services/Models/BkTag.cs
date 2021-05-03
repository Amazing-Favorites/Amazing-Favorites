using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record BkTag
    {
        public string Tag { get; set; }
        public Dictionary<TextAliasType, TextAlias> TagAlias { get; set; } = new();
    }
}