using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class CloudServiceFactory : ICloudServiceFactory
    {
        private readonly ILogger<CloudServiceFactory> _logger;
        private readonly IUserOptionsService _userOptionsService;
        private readonly ILifetimeScope _lifetimeScope;

        public CloudServiceFactory(
            ILogger<CloudServiceFactory> logger,
            IUserOptionsService userOptionsService,
            ILifetimeScope lifetimeScope)
        {
            _logger = logger;
            _userOptionsService = userOptionsService;
            _lifetimeScope = lifetimeScope;
            _lifetimeScope = lifetimeScope;
        }

        public async Task<ICloudService> CreateAsync()
        {
            var options = await _userOptionsService.GetOptionsAsync();
            var cloudBkProviderType = options.CloudBkFeature!.CloudBkProviderType;
            _logger.LogInformation("current provider: {CloudBkProviderType}", cloudBkProviderType);
            var service = _lifetimeScope.ResolveKeyed<ICloudService>(cloudBkProviderType);
            return service;
        }
    }
}