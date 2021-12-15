using System.Threading.Tasks;
using Newbe.BookmarkManager.WebApi;
using Refit;

namespace Newbe.BookmarkManager.Services;

public interface ICloudBkApi
{
    [Post("/bk")]
    Task<ApiResponse<SaveToCloudOutput>> SaveToCloudAsync(CloudBkCollection cloudBkCollection);

    [Get("/bk")]
    Task<ApiResponse<GetCloudOutput>> GetCloudBkAsync(long etagVersion);
}