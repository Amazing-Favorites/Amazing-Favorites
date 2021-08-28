using System;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Components
{
    public class ManagePageNotificationService : IManagePageNotificationService
    {
        private readonly IUserOptionsService _userOptionsService;
        private readonly IAfEventHub _afEventHub;

        public ManagePageNotificationService(
            IUserOptionsService userOptionsService,
            IAfEventHub afEventHub)
        {
            _userOptionsService = userOptionsService;
            _afEventHub = afEventHub;
        }

        public async Task RunAsync()
        {
            var userOptions = await _userOptionsService.GetOptionsAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));


            if (userOptions?.PinyinFeature?.Enabled == true &&
                userOptions.PinyinFeature?.ExpireDate.HasValue == true &&
                userOptions.PinyinFeature.ExpireDate < DateTime.Now.AddDays(Consts.JwtExpiredWarningDays))
            {
                await NoticeWarning("PinyinAccessToken");
            }

            if (userOptions is
                {
                    AcceptPrivacyAgreement: true,
                    CloudBkFeature:
                    {
                        Enabled: true
                    }
                })
            {
                var cloudBkFeature = userOptions.CloudBkFeature;
                switch (cloudBkFeature.CloudBkProviderType)
                {
                    case CloudBkProviderType.NewbeApi:
                        if (cloudBkFeature.ExpireDate.HasValue &&
                            cloudBkFeature.ExpireDate < DateTime.Now.AddDays(Consts.JwtExpiredWarningDays))
                        {
                            await NoticeWarning("CloudBkAccessToken");
                        }

                        break;
                    default:
                        break;
                }
            }

            if (userOptions is
                {
                    AcceptPrivacyAgreement: false
                })
            {
                var msg = userOptions.AcceptPrivacyAgreementBefore
                    ? "Privacy Agreement has been updated recently, please check it out in control panel"
                    : "We invite you to read our privacy agreement to enable more features in control panel";
                await _afEventHub.PublishAsync(new UserNotificationEvent
                {
                    AfNotificationType = AfNotificationType.Info,
                    Message = msg
                });
            }

            async Task NoticeWarning(string tokenName)
            {
                await _afEventHub.PublishAsync(new UserNotificationEvent
                {
                    AfNotificationType = AfNotificationType.Warning,
                    Message = $"{tokenName} is about to expire",
                    Description =
                        $"Your token will be expired within {Consts.JwtExpiredWarningDays} days, please try to create a new one.",
                });
            }
        }
    }
}