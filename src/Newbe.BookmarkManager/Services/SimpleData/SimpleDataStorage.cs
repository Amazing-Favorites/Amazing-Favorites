using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.SimpleData
{
    public class SimpleDataStorage : ISimpleDataStorage
    {
        private readonly IIndexedDbRepo<SimpleDataEntity, string> _repo;

        public SimpleDataStorage(
            IIndexedDbRepo<SimpleDataEntity, string> repo)
        {
            _repo = repo;
        }

        public async Task<T> GetOrDefaultAsync<T>() where T : ISimpleData, new()
        {
            var id = GetId(typeof(T));
            var entity = await _repo.GetAsync(id);
            if (entity == null)
            {
                return new T();
            }

            var re = await JsonHelper.DeserializeAsync<T>(entity.PayloadJson);
            return re!;
        }

        public async Task SaveAsync<T>(T data) where T : ISimpleData
        {
            var id = GetId(typeof(T));
            await _repo.UpsertAsync(new SimpleDataEntity
            {
                Id = id,
                PayloadJson = JsonSerializer.Serialize(data)
            });
        }

        private static string GetId(Type type)
        {
            return type.Name;
        }
    }
}