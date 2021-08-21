using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ICloudServiceFactory
    {
        Task<ICloudService> CreateAsync();
    }
}