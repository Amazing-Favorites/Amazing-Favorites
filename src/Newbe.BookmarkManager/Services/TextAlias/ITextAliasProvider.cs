using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public interface ITextAliasProvider
    {
        public TextAliasType TextAliasType { get; }
        bool NeedFill(BkTag tag);
        Task<AliasUpdateResult> FillAsync(BkTag[] tags);

        bool NeedFill(Bk bk);
        Task<AliasUpdateResult> FillAsync(Bk[] bks);
    }

    public record AliasUpdateResult
    {
        public bool IsOk { get; set; }
        public TextAliasType TextAliasType { get; set; }
    }
}