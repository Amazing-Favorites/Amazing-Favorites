using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ICloudServiceFactory
    {
        Task<ServiceItem> CreateAsync();
    }

    public record ServiceItem(CloudBkProviderType CloudBkProviderType,
        ICloudService CloudService)
    {
    }
}