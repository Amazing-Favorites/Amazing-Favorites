using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public class NewbeApiCloudService : ICloudService
    {
        private readonly ILogger<NewbeApiCloudService> _logger;
        private readonly ICloudBkApi _cloudBkApi;

        public NewbeApiCloudService(
            ILogger<NewbeApiCloudService> logger,
            ICloudBkApi cloudBkApi)
        {
            _logger = logger;
            _cloudBkApi = cloudBkApi;
        }

        public async Task<CloudBkStatus> GetCloudAsync(long etagVersion)
        {
            var response = await _cloudBkApi.GetCloudBkAsync(etagVersion);
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return new CloudBkStatus(false, default);
            }

            await response.EnsureSuccessStatusCodeAsync();
            return new CloudBkStatus(true, response.Content);
        }

        public async Task<SaveToCloudOutput> SaveToCloudAsync(CloudBkCollection cloudBkCollection)
        {
            var resp = await _cloudBkApi.SaveToCloudAsync(cloudBkCollection);
            await resp.EnsureSuccessStatusCodeAsync();
            return resp.Content!;
        }
    }
}