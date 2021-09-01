using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface INotificationRecordService
    {
        Task AddAsync(MsgItem item);
        Task<List<NotificationRecord>> GetListAsync();
        Task MakeAsReadAsync();
        Task<bool> GetNewMessageStatusAsync();
    }
}