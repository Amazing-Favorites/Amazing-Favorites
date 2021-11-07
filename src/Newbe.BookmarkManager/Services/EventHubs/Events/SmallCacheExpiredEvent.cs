namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record SmallCacheExpiredEvent : IAfEvent
    {
        public string CacheKey { get; set; }
    }
}