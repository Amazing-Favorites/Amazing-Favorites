using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
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
            var pinyinOutput = await _pinyinApi.GetPinyinAsync(new PinyinInput
            {
                Text = title.ToArray()
            });
            return pinyinOutput.IsOk ? pinyinOutput.Pinyin : new();
        }
    }
}