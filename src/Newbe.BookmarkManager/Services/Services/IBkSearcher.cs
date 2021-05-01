namespace Newbe.BookmarkManager.Services
{
    public interface IBkSearcher
    {
        SearchResultItem[] Search(string searchText, int limit);
    }
}