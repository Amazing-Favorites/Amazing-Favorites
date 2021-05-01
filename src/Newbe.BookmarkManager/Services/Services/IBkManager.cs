using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkManager
    {
        ValueTask AddClickAsync(string url, int moreCount);
        ValueTask RestoreAsync();

        ValueTask RemoveTagAsync(string url, string tag);
        ValueTask<bool> AddTagAsync(string url, string tag);
        ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags);
        ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls);
        Bk Get(string url);
    }
}