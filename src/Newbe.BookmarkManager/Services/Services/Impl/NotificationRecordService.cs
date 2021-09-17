using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Services
{
    public class NotificationRecordService : INotificationRecordService
    {
        private readonly IIndexedDbRepo<NotificationRecord, long> _notificationRepo;
        private readonly IClock _clock;
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IAfEventHub _afEventHub;

        public NotificationRecordService(IIndexedDbRepo<NotificationRecord, long> notificationRepo,
            IClock clock,
            ISimpleDataStorage simpleDataStorage,
            IAfEventHub afEventHub)
        {
            _notificationRepo = notificationRepo;
            _clock = clock;
            _simpleDataStorage = simpleDataStorage;
            _afEventHub = afEventHub;
        }

        public async Task AddAsync(MsgItem item)
        {
            var records = await _notificationRepo.GetAllAsync();
            var groupName = item.Type.ToString();
            var record = records.FirstOrDefault(x => x.Group == groupName);
            var now = _clock.UtcNow;
            item.CreatedTime = now;
            if (record == null)
            {
                record = new NotificationRecord
                {
                    Group = groupName,
                    Id = now,
                    Items = new List<MsgItem>
                    {
                        item
                    },
                    UpdateTime = now
                };
            }
            else
            {
                record.Items.Insert(0, item);
                while (record.Items.Count > 10)
                {
                    record.Items.RemoveAt(record.Items.Count - 1);
                }
                record.UpdateTime = now;
            }

            await _notificationRepo.UpsertAsync(record);
            var status = await _simpleDataStorage.GetOrDefaultAsync<NotificationCenterStatus>();
            status.NewMessage = true;
            await _simpleDataStorage.SaveAsync(status);
            await _afEventHub.PublishAsync(new NewNotificationEvent());
        }

        public async Task<List<NotificationRecord>> GetListAsync()
        {
            var items = await _notificationRepo.GetAllAsync();
            var re = items
                .OrderByDescending(a => a.UpdateTime)
                .Take(5)
                .ToList();
            return re;
        }

        public async Task MakeAsReadAsync()
        {
            var status = await _simpleDataStorage.GetOrDefaultAsync<NotificationCenterStatus>();
            status.NewMessage = false;
            await _simpleDataStorage.SaveAsync(status);
        }

        public async Task<bool> GetNewMessageStatusAsync()
        {
            var status = await _simpleDataStorage.GetOrDefaultAsync<NotificationCenterStatus>();
            return status.NewMessage;
        }

        private List<NotificationRecord> CreateStaticMessageList()
        {
            var rd = new Random();
            return new List<NotificationRecord>
            {
                new()
                {
                    Group = UserNotificationType.NewRelease.ToString(),
                    Id = rd.NextInt64(),
                    UpdateTime = _clock.UtcNow,
                    Items = new List<MsgItem>
                    {
                        new()
                        {
                            Title = "🆕 New version released!",
                            Message = "A new version of Amazing Favorites released.",
                            ArgsJson = JsonSerializer.Serialize(new NewReleaseInput
                            {
                                Version = $"v{Consts.CurrentVersion}",
                                WhatsNewUrl = "https://af.newbe.pro"
                            }),
                            Type = UserNotificationType.NewRelease,
                            CreatedTime = _clock.UtcNow
                        }
                    }
                },
                new()
                {
                    Group = UserNotificationType.Welcome.ToString(),
                    Id = rd.NextInt64(),
                    UpdateTime = _clock.UtcNow,
                    Items = new List<MsgItem>
                    {
                        new()
                        {
                            Title = "🌟 Welcome! My friend!",
                            Message =
                                "Thank you very much for installing this extension and we hope we can help you in the coming days. You can learn the basic usage of this extension by following the link, just have fun!",
                            Type = UserNotificationType.Welcome,
                            CreatedTime = _clock.UtcNow
                        }
                    }
                },
                new()
                {
                    Group = UserNotificationType.SyncDataWithCloud.ToString(),
                    Id = rd.NextInt64(),
                    UpdateTime = _clock.UtcNow,
                    Items = new List<MsgItem>
                    {
                        new()
                        {
                            Title = "☁ Time to sync your data with cloud",
                            Message =
                                "You haven`t sync your data with cloud for a long time, please click the button to keep your data up to date with cloud."
                        }
                    }
                },
                new()
                {
                    Group = UserNotificationType.PrivacyAgreementUpdated.ToString(),
                    Id = rd.NextInt64(),
                    UpdateTime = _clock.UtcNow,
                    Items = new List<MsgItem>
                    {
                        new()
                        {
                            Title = "⚖ Privacy Agreement updated",
                            Message =
                                "User Privacy Agreement has been updated recently. please review the agreement before you use some cloud-related feature"
                        }
                    }
                }
            };
        }
    }
}