using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Tests
{
    public class StaticClock : IClock
    {
        public long UtcNow { get; set; }
    }
}