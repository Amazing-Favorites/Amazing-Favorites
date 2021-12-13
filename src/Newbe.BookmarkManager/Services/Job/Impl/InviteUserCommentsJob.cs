using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class InviteUserCommentsJob:IInviteUserCommentsJob
    {
        private readonly IUserOptionsService _userOptionsService;
        private readonly INewNotification _newNotification;
        private readonly ILogger<InviteAcceptPrivacyAgreementJob> _logger;
        public async ValueTask StartAsync()
        {
            var userOptions = await _userOptionsService.GetOptionsAsync();
            if (userOptions.InvitationTime is null ||
                userOptions.InvitationTime >= DateTime.Now.AddDays(-Consts.InviteUserCommentsCdDays))
            {
                userOptions.InvitationTime = DateTime.UtcNow;
                await _userOptionsService.SaveAsync(userOptions);
                await _newNotification.InviteUserCommentsAsync();
            }
        }
    }
}