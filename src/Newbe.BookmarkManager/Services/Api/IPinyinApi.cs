using System;
using System.Threading.Tasks;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IPinyinApi
    {
        [Post("/pinyin")]
        Task<ApiResponse<PinyinOutput>> GetPinyinAsync(PinyinInput input);
    }
}