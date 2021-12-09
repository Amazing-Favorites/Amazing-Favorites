using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services;

public static class IndexedBkExtensions
{
    public static async Task<T?> GetSingleOneAsync<T>(this IIndexedDbRepo<T, string> repo) where T : IEntity<string>
    {
        var re = await repo.GetAsync(Consts.SingleOneDataId);
        return re;
    }
}