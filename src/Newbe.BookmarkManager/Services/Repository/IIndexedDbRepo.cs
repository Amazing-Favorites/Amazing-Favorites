using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IIndexedDbRepo<T, in TKey>
        where T : IEntity<TKey>
    {
        Task<List<T>> GetAllAsync();
        Task UpsertAsync(T entity);
        Task<T?> GetAsync(TKey id);
        Task DeleteAsync(TKey id);
        Task DeleteAllAsync();
    }
}