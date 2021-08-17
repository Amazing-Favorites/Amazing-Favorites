using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IRecordService
    {
        Task AddAsync(UserClickRecord userClickRecord);
    }
}