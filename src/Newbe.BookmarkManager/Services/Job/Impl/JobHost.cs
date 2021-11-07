using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class JobHost : IJobHost
    {
        private readonly ILogger<JobHost> _logger;
        private readonly ISyncBookmarkJob _syncBookmarkJob;
        private readonly ISyncAliasJob _syncAliasJob;
        private readonly ISyncCloudJob _syncCloudJob;
        private readonly IDataFixJob _dataFixJob;
        private readonly ISyncTagRelatedBkCountJob _syncTagRelatedBkCountJob;
        private readonly IShowWhatNewJob _showWhatNewJob;
        private readonly IShowWelcomeJob _showWelcomeJob;
        private readonly ISyncCloudStatusCheckJob _syncCloudStatusCheckJob;
        private readonly IInviteAcceptPrivacyAgreementJob _inviteAcceptPrivacyAgreementJob;
        private readonly IHandleUserClickIconJob _handleUserClickIconJob;
        private readonly IHandleOmniBoxSuggestJob _handleOmniBoxSuggestJob;
        private readonly IBkSearcherServerJob _bkSearcherServer;
        public JobHost(
            ILogger<JobHost> logger,
            ISyncBookmarkJob syncBookmarkJob,
            ISyncAliasJob syncAliasJob,
            ISyncCloudJob syncCloudJob,
            IDataFixJob dataFixJob,
            ISyncTagRelatedBkCountJob syncTagRelatedBkCountJob,
            IShowWhatNewJob showWhatNewJob,
            IShowWelcomeJob showWelcomeJob,
            ISyncCloudStatusCheckJob syncCloudStatusCheckJob,
            IInviteAcceptPrivacyAgreementJob inviteAcceptPrivacyAgreementJob,
            IHandleUserClickIconJob handleUserClickIconJob,
            IHandleOmniBoxSuggestJob handleOmniBoxSuggestJob,
            IBkSearcherServerJob bkSearcherServer)
        {
            _logger = logger;
            _syncBookmarkJob = syncBookmarkJob;
            _syncAliasJob = syncAliasJob;
            _syncCloudJob = syncCloudJob;
            _dataFixJob = dataFixJob;
            _syncTagRelatedBkCountJob = syncTagRelatedBkCountJob;
            _showWhatNewJob = showWhatNewJob;
            _showWelcomeJob = showWelcomeJob;
            _syncCloudStatusCheckJob = syncCloudStatusCheckJob;
            _inviteAcceptPrivacyAgreementJob = inviteAcceptPrivacyAgreementJob;
            _handleUserClickIconJob = handleUserClickIconJob;
            _handleOmniBoxSuggestJob = handleOmniBoxSuggestJob;
            _bkSearcherServer = bkSearcherServer;
        }

        public async Task StartAsync()
        {
            foreach (var job in GetJobs())
            {
                await RunCoreAsync(job);
            }
            _logger.LogInformation("All job started");

            async Task RunCoreAsync(IJob job)
            {
                await job.StartAsync();
                _logger.LogInformation("{Job} Started", job.GetType().Name);
            }

            IEnumerable<IJob> GetJobs()
            {
                yield return _dataFixJob;
                yield return _handleUserClickIconJob;
                yield return _handleOmniBoxSuggestJob;
                yield return _bkSearcherServer;
                yield return _showWelcomeJob;
                yield return _showWhatNewJob;
                yield return _syncBookmarkJob;
                yield return _syncAliasJob;
                yield return _syncCloudJob;
                yield return _syncTagRelatedBkCountJob;
                yield return _syncCloudStatusCheckJob;
                yield return _inviteAcceptPrivacyAgreementJob;
            }
        }
    }
}