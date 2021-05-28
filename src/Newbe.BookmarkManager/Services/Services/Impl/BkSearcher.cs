using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class BkSearcher : IBkSearcher
    {
        private readonly ILogger<BkSearcher> _logger;
        private readonly IBkDataHolder _bkDataHolder;
        private readonly IBookmarkDataHolder _bookmarkDataHolder;

        public BkSearcher(
            ILogger<BkSearcher> logger,
            IBkDataHolder bkDataHolder,
            IBookmarkDataHolder bookmarkDataHolder)
        {
            _logger = logger;
            _bkDataHolder = bkDataHolder;
            _bookmarkDataHolder = bookmarkDataHolder;
        }

        public async Task InitAsync()
        {
            await _bkDataHolder.StartAsync();
            await _bookmarkDataHolder.StartAsync();
        }

        public SearchResultItem[] Search(string searchText, int limit)
        {
            var source = _bkDataHolder.Collection.Bks.Values;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return source
                    .Where(FilterByNode())
                    .OrderByDescending(x => x.LastClickTime)
                    .ThenByDescending(x => x.ClickedCount)
                    .Take(limit)
                    .Select(x =>
                    {
                        var r = new SearchResultItem(x);
                        r.AddScore(ScoreReason.Const, 10);
                        return r;
                    })
                    .ToArray();
            }

            var input = SearchInput.Parse(searchText);
            _logger.LogInformation(
                "Search text parse result, SourceText: {SearchInput} Keywords: {Keywords}, Tags: {Tags}",
                input.SourceText,
                input.Keywords,
                input.Tags);

            var matchTags = _bkDataHolder.Collection.Tags
                .Where(tag =>
                    input.Tags.Contains(tag.Key) || input.Keywords.Any(keyword => StringContains(tag.Key, keyword)))
                .Select(x => x.Key)
                .ToHashSet();

            var matchTagAlias = _bkDataHolder.Collection.Tags
                .Where(tag => input.Keywords.Any(keyword =>
                    tag.Value.TagAlias.Values.Any(tagAlias => StringContains(tagAlias.Alias, keyword))))
                .Select(x => x.Key)
                .ToHashSet();

            var re = source
                .Where(FilterByNode())
                .Select(MatchBk)
                .Where(x => x.Matched)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.ClickCount)
                .Take(limit)
                .ToArray();

            return re;

            SearchResultItem MatchBk(Bk item)
            {
                var result = new SearchResultItem(item)
                {
                    ClickCount = item.ClickedCount
                };

                result.AddScore(ScoreReason.Title, input.Keywords.Any(x => StringContains(item.Title, x)));

                result.AddScore(ScoreReason.TitleAlias, item.TitleAlias?.Values != null &&
                                                        item.TitleAlias.Values.Any(al =>
                                                            input.Keywords.Any(x => StringContains(al.Alias, x))));

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

        private Func<Bk, bool> FilterByNode()
        {
            return bks => _bookmarkDataHolder.Nodes.ContainsKey(bks.Url);
        }
    }

    public record SearchInput
    {
        public static SearchInput Parse(string searchText)
        {
            var searchInput = new SearchInput
            {
                SourceText = searchText
            };
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return searchInput;
            }

            var keywords = searchText
                .Split(" ")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var tags = keywords.Where(x => x.StartsWith("t:")).ToArray();
            var tagSearchValues = tags.Select(x => x[2..]).ToArray();
            keywords = keywords.Except(tags).ToArray();
            searchInput.Keywords = keywords;
            searchInput.Tags = tagSearchValues;
            return searchInput;
        }

        public string SourceText { get; set; }
        public string[] Keywords { get; set; }
        public string[] Tags { get; set; }
    }
}