using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services;

public class PinyinTextAliasProvider : TextAliasProviderBase
{
    private readonly IPinyinApi _pinyinApi;
    public override TextAliasType TextAliasType => TextAliasType.Pinyin;

    public PinyinTextAliasProvider(IClock clock,
        ILogger<PinyinTextAliasProvider> logger,
        IPinyinApi pinyinApi) : base(clock, logger)
    {
        _pinyinApi = pinyinApi;
    }


    protected override async Task<Dictionary<string, string>> GetAliasCoreAsync(IEnumerable<string> title)
    {
        var repo = await _pinyinApi.GetPinyinAsync(new PinyinInput
        {
            Text = title.ToArray()
        });
        if (repo.IsSuccessStatusCode && repo.Content != null)
        {
            var pinyinOutput = repo.Content;
            return pinyinOutput.IsOk ? pinyinOutput.Pinyin : new();
        }

        return new Dictionary<string, string>();
    }
}