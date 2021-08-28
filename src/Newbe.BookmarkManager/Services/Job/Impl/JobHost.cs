using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class JobHost : IJobHost
    {
        private readonly ISyncBookmarkJob _syncBookmarkJob;
        private readonly ISyncAliasJob _syncAliasJob;
        private readonly ISyncCloudJob _syncCloudJob;
        private readonly IDataFixJob _dataFixJob;
        private readonly ISyncTagRelatedBkCountJob _syncTagRelatedBkCountJob;
        private readonly IShowWhatNewJob _showWhatNewJob;
        private readonly IShowWelcomeJob _showWelcomeJob;
        private readonly ISyncCloudStatusCheckJob _syncCloudStatusCheckJob;

        public JobHost(ISyncBookmarkJob syncBookmarkJob,
            ISyncAliasJob syncAliasJob,
            ISyncCloudJob syncCloudJob,
            IDataFixJob dataFixJob,
            ISyncTagRelatedBkCountJob syncTagRelatedBkCountJob,
            IShowWhatNewJob showWhatNewJob,
            IShowWelcomeJob showWelcomeJob,
            ISyncCloudStatusCheckJob syncCloudStatusCheckJob)
        {
            _syncBookmarkJob = syncBookmarkJob;
            _syncAliasJob = syncAliasJob;
            _syncCloudJob = syncCloudJob;
            _dataFixJob = dataFixJob;
            _syncTagRelatedBkCountJob = syncTagRelatedBkCountJob;
            _showWhatNewJob = showWhatNewJob;
            _showWelcomeJob = showWelcomeJob;
            _syncCloudStatusCheckJob = syncCloudStatusCheckJob;
        }

        public async Task StartAsync()
        {
            await _dataFixJob.StartAsync();
            await _showWelcomeJob.StartAsync();
            await _showWhatNewJob.StartAsync();
            await _syncBookmarkJob.StartAsync();
            await _syncAliasJob.StartAsync();
            await _syncCloudJob.StartAsync();
            await _syncTagRelatedBkCountJob.StartAsync();
            await _syncCloudStatusCheckJob.StartAsync();
        }
    }
}