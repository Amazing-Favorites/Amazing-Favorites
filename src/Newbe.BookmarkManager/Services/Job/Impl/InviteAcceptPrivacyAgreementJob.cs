using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class InviteAcceptPrivacyAgreementJob : IInviteAcceptPrivacyAgreementJob
    {
        private readonly IUserOptionsService _userOptionsService;
        private readonly INewNotification _newNotification;
        private readonly ILogger<InviteAcceptPrivacyAgreementJob> _logger;

        public InviteAcceptPrivacyAgreementJob(
            IUserOptionsService userOptionsService,
            INewNotification newNotification,
            ILogger<InviteAcceptPrivacyAgreementJob> logger)
        {
            _userOptionsService = userOptionsService;
            _newNotification = newNotification;
            _logger = logger;
        }

        public async ValueTask StartAsync()
        {
            var userOptions = await _userOptionsService.GetOptionsAsync();
            if (userOptions is
                {
                    AcceptPrivacyAgreement: false
                })
            {
                _logger.LogInformation("User haven't accept privacy agreement");
                await _newNotification.PrivacyAgreementUpdateAsync();
            }
        }
    }
}