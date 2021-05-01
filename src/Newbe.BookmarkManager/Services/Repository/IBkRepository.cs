using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkRepository
    {
        ValueTask<long> GetLateUpdateTimeAsync();
        ValueTask<BkEntityCollection> GetLatestDataAsync();
        ValueTask SaveAsync(BkEntityCollection collection);
    }
}