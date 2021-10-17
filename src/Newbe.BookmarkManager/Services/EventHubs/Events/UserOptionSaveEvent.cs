using WebExtensions.Net.BrowsingData;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserOptionSaveEvent : IAfEvent
    {
        public UserOptions UserOptions { get; set; }

        public bool OminiboxSuggestChanged { get; set; }
    }
}