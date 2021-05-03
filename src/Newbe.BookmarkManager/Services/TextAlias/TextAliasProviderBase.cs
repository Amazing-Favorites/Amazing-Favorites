using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public abstract class TextAliasProviderBase : ITextAliasProvider
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;
        public abstract TextAliasType TextAliasType { get; }

        protected TextAliasProviderBase(
            IClock clock,
            ILogger logger)
        {
            _clock = clock;
            _logger = logger;
        }


        public bool NeedFill(BkTag tag)
        {
            return !tag.TagAlias.ContainsKey(TextAliasType);
        }

        public async Task<AliasUpdateResult> FillAsync(BkTag[] tags)
        {
            var re = new AliasUpdateResult
            {
                TextAliasType = TextAliasType,
            };

            var updated = false;
            var tagTexts = tags.Select(x => x.Tag).ToArray();
            var tagAlias = await GetAliasAsync(tagTexts);
            foreach (var tag in tags)
            {
                if (tagAlias.TryGetValue(tag.Tag, out var alias))
                {
                    tag.TagAlias ??= new Dictionary<TextAliasType, TextAlias>();
                    tag.TagAlias[TextAliasType] = new TextAlias
                    {
                        Alias = alias,
                        LastUpdatedTime = _clock.UtcNow,
                        TextAliasType = TextAliasType
                    };
                    updated = true;
                }
            }

            re.IsOk = updated;
            return re;
        }

        public bool NeedFill(Bk bk)
        {
            if (bk.TitleAlias != null &&
                bk.TitleAlias.TryGetValue(TextAliasType, out var alias) &&
                alias.LastUpdatedTime >= bk.TitleLastUpdateTime)
            {
                return false;
            }

            return true;
        }

        public async Task<AliasUpdateResult> FillAsync(Bk[] bks)
        {
            var re = new AliasUpdateResult
            {
                TextAliasType = TextAliasType,
            };

            var titles = bks.Select(x => x.Title).ToArray();
            var titleAliasDict = await GetAliasAsync(titles);

            var updated = false;
            foreach (var bk in bks)
            {
                if (titleAliasDict.TryGetValue(bk.Title, out var newAliasText))
                {
                    var alias = new TextAlias
                    {
                        TextAliasType = TextAliasType,
                        LastUpdatedTime = _clock.UtcNow,
                        Alias = newAliasText
                    };
                    bk.TitleAlias ??= new();
                    bk.TitleAlias[TextAliasType] = alias;
                    updated = true;
                }
                else
                {
                    _logger.LogInformation("There is not found alias from provider for {Title}", bk.Title);
                }
            }

            re.IsOk = updated;

            return re;
        }

        private async Task<Dictionary<string, string>> GetAliasAsync(IEnumerable<string> title)
        {
            try
            {
                var re = await GetAliasCoreAsync(title);
                return re;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get alias");
                return new();
            }
        }

        protected abstract Task<Dictionary<string, string>> GetAliasCoreAsync(IEnumerable<string> title);
    }
}