using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class NotificationRecordService : INotificationRecordService
    {
        private readonly IIndexedDbRepo<NotificationRecord, long> _notificationRepo;
        private readonly IClock _clock;

        public NotificationRecordService(IIndexedDbRepo<NotificationRecord, long> notificationRepo, IClock clock)
        {
            _notificationRepo = notificationRepo;
            _clock = clock;
        }

        public async Task AddAsync(NotificationRecord record)
        {
            record.Id = _clock.UtcNow;
            await _notificationRepo.UpsertAsync(record);
        }

        public async Task<List<NotificationRecord>> GetListAsync()
        {
            return (await _notificationRepo.GetAllAsync())
                .OrderByDescending(a=>a.CreatedTime)
                .ThenBy(a=>a.Read)
                .Take(5)
                .ToList();
        }

        public async Task CompleteAsync(long id)
        {
            var record = await _notificationRepo.GetAsync(id);
            if (record != null)
            {
                record.CompletionTime = DateTime.UtcNow;
                record.Read = true;
                await _notificationRepo.UpsertAsync(record);
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var result = (await _notificationRepo.GetAllAsync())
                    .Count(a => !a.Read);
                return result;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}