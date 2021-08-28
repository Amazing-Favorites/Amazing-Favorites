using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IJobHost
    {
        public Task StartAsync();
    }
}