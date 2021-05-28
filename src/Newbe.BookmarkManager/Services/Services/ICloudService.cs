using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface ICloudService
    {
        Task<CloudBkStatus> GetCloudAsync(long etagVersion);
        Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection);
    }
}