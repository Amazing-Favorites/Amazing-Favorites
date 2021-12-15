namespace Newbe.BookmarkManager.Services;

public interface IEntity<out TKey>
{
    public TKey Id { get; }
}