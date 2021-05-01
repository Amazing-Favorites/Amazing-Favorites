using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record BkEntityCollection
    {
        public int Version { get; set; }
        public long LastUpdateTime { get; set; }
        public Dictionary<string, Bk> Bks { get; set; } = new();
    }
}