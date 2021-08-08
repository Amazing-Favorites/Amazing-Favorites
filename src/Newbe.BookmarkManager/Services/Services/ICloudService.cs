using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface ICloudService
    {
        Task<CloudBkStatus> GetCloudAsync(long etagVersion);

        [Insight(EventName = "Bk Local To Cloud Event")]
        Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection);
    }
}