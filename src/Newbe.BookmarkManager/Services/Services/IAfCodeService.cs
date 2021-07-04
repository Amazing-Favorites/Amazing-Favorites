using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IAfCodeService
    {
        Task<string> CreateAfCodeAsync(string url, AfCodeType codeType);
        Task<bool> TryParseAsync(string source, out AfCodeResult? afCodeResult);
    }
}