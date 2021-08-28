namespace Newbe.BookmarkManager.Services.EventHubs
{
    public record UserLoginSuccessEvent(
        CloudBkProviderType CloudBkProviderType,
        string AccessToken) : IAfEvent
    {
    }
}