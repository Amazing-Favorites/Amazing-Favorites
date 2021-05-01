using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class TitleAliasProvider : ITitleAliasProvider
    {
        public Task<Dictionary<BkAliasType, string>> GetTitleAliasAsync(string title)
        {
            return Task.FromResult(new Dictionary<BkAliasType, string>());
        }
    }
}