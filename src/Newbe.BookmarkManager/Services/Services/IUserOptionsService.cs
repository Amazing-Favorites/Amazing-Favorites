using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IUserOptionsService
    {
        ValueTask<UserOptions> GetOptionsAsync();
        ValueTask SaveAsync(UserOptions options);
    }
}