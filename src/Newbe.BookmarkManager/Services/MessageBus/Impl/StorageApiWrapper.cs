using System.Text.Json;
using System.Threading.Tasks;
using WebExtensions.Net.Storage;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public class StorageApiWrapper : IStorageApiWrapper
    {
        private readonly IStorageApi _storageApi;

        public StorageApiWrapper(
            IStorageApi storageApi)
        {
            _storageApi = storageApi;
        }

        public ValueTask RegisterCallBack(StorageChangeCallback callback)
        {
            return _storageApi.OnChanged.AddListener((o, s) => callback.Invoke((JsonElement)o, s));
        }

        public async ValueTask SetLocal(object value)
        {
            var local = await _storageApi.GetLocal();
            await local.Set(value);
        }
    }
}