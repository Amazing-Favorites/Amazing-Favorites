using System;
using System.Threading.Tasks;
using AntDesign;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Components
{
    public class ManagePageNotificationService : IManagePageNotificationService
    {
        private readonly IUserOptionsService _userOptionsService;
        private readonly NotificationService _notificationService;

        public ManagePageNotificationService(
            IUserOptionsService userOptionsService,
            NotificationService notificationService)
        {
            _userOptionsService = userOptionsService;
            _notificationService = notificationService;
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

            if (userOptions?.CloudBkFeature?.Enabled == true &&
                userOptions.CloudBkFeature?.ExpireDate.HasValue == true &&
                userOptions.CloudBkFeature.ExpireDate < DateTime.Now.AddDays(Consts.JwtExpiredWarningDays))
            {
                await NoticeWarning("CloudBkAccessToken");
            }

            if (userOptions is
            {
                AcceptPrivacyAgreement: false
            })
            {
                var msg = userOptions.AcceptPrivacyAgreementBefore
                    ? "Privacy Agreement has been updated recently, please check it out in control panel"
                    : "We invite you to read our privacy agreement to enable more features in control panel";
                await _notificationService.Info(new NotificationConfig
                {
                    Message = msg
                });
            }

            async Task NoticeWarning(string tokenName)
            {
                await _notificationService.Warning(new NotificationConfig()
                {
                    Message = $"{tokenName} is about to expire",
                    Description =
                        $"Your token will be expired within {Consts.JwtExpiredWarningDays} days, please try to create a new one.",
                });
            }
        }
    }
}