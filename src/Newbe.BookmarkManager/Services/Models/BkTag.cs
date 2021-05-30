using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.Tags)]
    public record BkTag : IEntity<string>
    {
        public string Id => Tag;
        public string Tag { get; init; }
        public Dictionary<TextAliasType, TextAlias> TagAlias { get; set; } = new();
    }
}