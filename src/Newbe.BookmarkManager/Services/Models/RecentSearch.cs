using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.RecentSearch)]
    public record RecentSearch : IEntity<string>
    {
        public string Id { get; set; } = Consts.SingleOneDataId;
        public List<RecentSearchItem> Items { get; set; }
    }

    public record RecentSearchItem
    {
        public string Text { get; set; }
        public long LastTime { get; set; }
        public int SearchCount { get; set; }
    }
}