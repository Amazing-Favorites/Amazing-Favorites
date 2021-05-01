using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record BkTag
    {
        public string Tag { get; set; }
        public Dictionary<BkAliasType, string> TagAlias { get; set; } = new();
    }
}