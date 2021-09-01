using System;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services;

namespace Newbe.BookmarkManager.Components
{
    public class ManagePageNotificationService : IManagePageNotificationService
    {
        private readonly IUserOptionsService _userOptionsService;
        private readonly INewNotification _newNotification;

        public ManagePageNotificationService(
            IUserOptionsService userOptionsService,
            INewNotification newNotification)
        {
            _userOptionsService = userOptionsService;
            _newNotification = newNotification;
        }

        public async Task RunAsync()
        {
            var userOptions = await _userOptionsService.GetOptionsAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));

            var now = DateTime.Now;
            if (userOptions?.PinyinFeature?.Enabled == true &&
                userOptions.PinyinFeature?.ExpireDate.HasValue == true &&
                userOptions.PinyinFeature.ExpireDate < now.AddDays(Consts.JwtExpiredWarningDays))
            {
                var days = (userOptions.PinyinFeature.ExpireDate.Value - now).Days;
                if (Math.Abs(days) < Consts.JwtExpiredWarningDays)
                {
                    await _newNotification.PinyinTokenExpiredAsync(new PinyinTokenExpiredInput
                    {
                        LeftDays = days
                    });
                }
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
                            cloudBkFeature.ExpireDate < now.AddDays(Consts.JwtExpiredWarningDays))
                        {
                            var days = (cloudBkFeature.ExpireDate.Value - now).Days;
                            if (Math.Abs(days) < Consts.JwtExpiredWarningDays)
                            {
                                await _newNotification.CloudBkTokenExpiredAsync(new CloudBkTokenExpiredInput
                                {
                                    LeftDays = days
                                });
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }
    }
}