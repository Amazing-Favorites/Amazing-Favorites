using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.SimpleData
{
    public interface ISimpleDataStorage
    {
        Task<T> GetOrDefaultAsync<T>()
            where T : ISimpleData, new();

        Task SaveAsync<T>(T data)
            where T : ISimpleData;
    }
}