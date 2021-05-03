using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class TitleAliasProvider
    {
        public Task<Dictionary<TextAliasType, string>> GetTitleAliasAsync(string title)
        {
            return Task.FromResult(new Dictionary<TextAliasType, string>());
        }
    }
}