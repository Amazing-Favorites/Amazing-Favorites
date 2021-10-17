using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public interface IStorageApiWrapper
    {
        ValueTask RegisterCallBack(StorageChangeCallback callback);
        ValueTask SetLocal(object value);
    }
}