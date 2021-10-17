namespace Newbe.BookmarkManager.Services.MessageBus
{
    public record BusOptions
    {
        public string EnvelopName { get; set; } = null!;
    }
}