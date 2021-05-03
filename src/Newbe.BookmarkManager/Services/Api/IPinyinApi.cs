using System.Threading.Tasks;
using Refit;

namespace Newbe.BookmarkManager.Services
{
    public interface IPinyinApi
    {
        [Post("/pinyin")]
        Task<PinyinOutput> GetPinyinAsync(PinyinInput input);
    }
}