using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services;

public interface IJob
{
    ValueTask StartAsync();
}