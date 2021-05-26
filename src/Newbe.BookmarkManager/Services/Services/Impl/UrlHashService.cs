using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Newbe.BookmarkManager.Services
{
    public class UrlHashService : IUrlHashService
    {
        public string GetHash(string url)
        {
            using var sha256 = new SHA256Managed();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(url));
            var sb = new StringBuilder();
            foreach (var b in hash.Take(4))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}