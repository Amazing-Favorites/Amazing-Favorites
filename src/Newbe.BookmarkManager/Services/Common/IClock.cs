namespace Newbe.BookmarkManager.Services;

public interface IClock
{
    /// <summary>
    /// Unix time seconds
    /// </summary>
    public long UtcNow { get; }
}