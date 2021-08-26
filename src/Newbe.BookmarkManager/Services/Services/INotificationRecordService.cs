using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface INotificationRecordService
    {
        Task AddAsync(NotificationRecord record);

        Task<List<NotificationRecord>> GetListAsync();
        
        Task CompleteAsync(long id);
        Task<int> GetUnreadCountAsync();
    }
}