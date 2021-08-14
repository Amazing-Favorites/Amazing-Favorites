using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.Ai;
using static Newbe.BookmarkManager.Services.Ai.Events;

namespace Newbe.BookmarkManager.Services
{
    public interface IUserOptionsService
    {
        ValueTask<UserOptions> GetOptionsAsync();

        [Insight(EventName = UserOptionSaveEvent)]
        Task SaveAsync(UserOptions options);
    }
}