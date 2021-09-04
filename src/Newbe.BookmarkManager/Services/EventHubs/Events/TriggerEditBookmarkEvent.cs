using Newtonsoft.Json;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record TriggerEditBookmarkEvent : IAfEvent
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public int TabId { get; set; }
    }
}