namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserClickAfIconEvent : IAfEvent
    {
        public int TabId { get; set; }
    }
}