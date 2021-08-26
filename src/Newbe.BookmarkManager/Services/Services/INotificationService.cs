using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface INotificationService
    {
        Task AddAsync(NotificationRecord record);

        Task<List<NotificationRecord>> GetPage();
         
        Task CompleteAsync(long id);
        Task<int> GetUnreadCountAsync();
    }
}