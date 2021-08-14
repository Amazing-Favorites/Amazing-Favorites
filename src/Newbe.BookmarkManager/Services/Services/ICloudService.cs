using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.WebApi;
using static Newbe.BookmarkManager.Services.Ai.Events;

namespace Newbe.BookmarkManager.Services
{
    public interface ICloudService
    {
        Task<CloudBkStatus> GetCloudAsync(long etagVersion);

        [Insight(EventName = BkLocalToCloudEvent)]
        Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection);
    }
}