using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ITagsManager
    {
        Task<List<BkTag>> GetHotAsync();
        Task AddCountAsync(string tag, int count);
        Task<string[]> GetAllTagsAsync();
        Task UpdateRelatedCountAsync(Dictionary<string, int> counts);
    }
}