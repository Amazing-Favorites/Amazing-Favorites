using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;

namespace Newbe.BookmarkManager.Services
{
    public interface IUserOptionsService
    {
        ValueTask<UserOptions> GetOptionsAsync();

        [Insight(EventName = "User Option Save Event")]
        Task SaveAsync(UserOptions options);
    }
}