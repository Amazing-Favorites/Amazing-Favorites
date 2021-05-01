using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ITitleAliasProvider
    {
        Task<Dictionary<BkAliasType, string>> GetTitleAliasAsync(string title);
    }
}