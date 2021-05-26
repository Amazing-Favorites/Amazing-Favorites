using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface IUserOptionsRepository
    {
        ValueTask<UserOptions> GetOptionsAsync();
        ValueTask SaveAsync(UserOptions options);
    }

    public record UserOptions
    {
        public PinyinFeature? PinyinFeature { get; set; }
        public CloudBkFeature? CloudBkFeature { get; set; }
    }

    public record PinyinFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }
    }

    public record CloudBkFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }
    }
}