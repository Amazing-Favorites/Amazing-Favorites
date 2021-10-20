using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlazorApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class IndexedBkSearcher : IBkSearcher
    {
        private readonly IApplicationInsights _insights;
        private readonly IIndexedDbRepo<Bk, string> _bkRepo;
        private readonly ILogger<IndexedBkSearcher> _logger;
        private readonly IIndexedDbRepo<BkTag, string> _tagRepo;

        public IndexedBkSearcher(
            IApplicationInsights insights,
            ILogger<IndexedBkSearcher> logger,
            IIndexedDbRepo<Bk, string> bkRepo,
            IIndexedDbRepo<BkTag, string> tagRepo)
        {
            _insights = insights;
            _bkRepo = bkRepo;
            _logger = logger;
            _tagRepo = tagRepo;
        }

        public virtual async Task<SearchResultItem[]> Search(string searchText, int limit)
        {
            var sw = Stopwatch.StartNew();
            var source = await SearchCore(searchText);
            if (string.IsNullOrWhiteSpace(searchText))
            {
                source = source
                    .OrderByDescending(x => x.LastClickTime)
                    .ThenByDescending(x => x.ClickCount)
                    .Take(limit)
                    .ToArray();
            }
            else
            {
                source = source
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.LastClickTime)
                    .ThenByDescending(x => x.ClickCount)
                    .Take(limit)
                    .ToArray();
            }
            var time = sw.ElapsedMilliseconds;
            _logger.LogInformation("Search cost: {Time} ms", time);
            return source.ToArray();


        }
        private async Task<IEnumerable<SearchResultItem>> SearchCore(string searchText)
        {
            var source = await _bkRepo.GetAllAsync();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return source
                    .Select(x =>
                    {
                        var r = new SearchResultItem(x)
                        {
                            LastClickTime = x.LastClickTime
                        };
                        r.AddScore(ScoreReason.Const, 10);
                        return r;
                    });
            }

            var input = SearchInput.Parse(searchText);
            _logger.LogInformation(
                "Search text parse result, SourceText: {SearchInput} Keywords: {Keywords}, Tags: {Tags}",
                input.SourceText,
                input.Keywords,
                input.Tags);

            var tags = await _tagRepo.GetAllAsync();
            var tagDict = tags.ToDictionary(x => x.Tag);
            var matchTags = tagDict
                .Where(tag =>
                    input.Tags.Contains(tag.Key) ||
                    input.Keywords.Any(keyword => StringContains(tag.Key, keyword)))
                .Select(x => x.Key)
                .ToHashSet();

            var matchTagAlias = tagDict
                .Where(tag => input.Keywords.Any(keyword =>
                    tag.Value.TagAlias.Values.Any(tagAlias => StringContains(tagAlias.Alias, keyword))))
                .Select(x => x.Key)
                .ToHashSet();

            var re = source
                .Select(MatchBk)
                .Where(x => x.Matched);

            return re;

            SearchResultItem MatchBk(Bk item)
            {
                var result = new SearchResultItem(item)
                {
                    ClickCount = item.ClickedCount,
                    LastClickTime = item.LastClickTime
                };

                result.AddScore(ScoreReason.Title, input.Keywords.Any(x => StringContains(item.Title, x)));

                result.AddScore(ScoreReason.TitleAlias, item.TitleAlias?.Values != null &&
                                                        item.TitleAlias.Values.Any(al =>
                                                            input.Keywords.Any(x =>
                                                                StringContains(al.Alias, x))));

                result.AddScore(ScoreReason.Url, input.Keywords.Any(x => StringContains(item.Url, x)));
                if (item.Tags?.Any() == true)
                {
                    result.AddScore(ScoreReason.Tags, item.Tags.Any(matchTags.Contains));
                    result.AddScore(ScoreReason.TagAlias, item.Tags.Any(matchTagAlias.Contains));
                }

                if (result.Score > 0)
                {
                    result.AddScore(ScoreReason.ClickCount, item.ClickedCount);
                }

                return result;
            }

            static bool StringContains(string a, string target)
            {
                return !string.IsNullOrWhiteSpace(a) && a.Contains(target, StringComparison.OrdinalIgnoreCase);
            }
        }

        public async Task<SearchResultItem[]> History(int limit)
        {
            var sw = Stopwatch.StartNew();
            var source = await SearchCore(string.Empty);
            source = source
                .Where(a => a.LastClickTime > 0)
                .OrderByDescending(x => x.LastClickTime)
                .ThenByDescending(x => x.ClickCount)
                .Take(limit);
            var time = sw.ElapsedMilliseconds;
            _logger.LogInformation("Search cost: {Time} ms", time);
            return source.ToArray();
        }
    }
}