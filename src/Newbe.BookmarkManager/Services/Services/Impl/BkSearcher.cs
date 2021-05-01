using System;
using System.Linq;

namespace Newbe.BookmarkManager.Services
{
    public class BkSearcher : IBkSearcher
    {
        private readonly IBkDataHolder _bkDataHolder;
        private readonly IBookmarkDataHolder _bookmarkDataHolder;

        public BkSearcher(IBkDataHolder bkDataHolder,
            IBookmarkDataHolder bookmarkDataHolder)
        {
            _bkDataHolder = bkDataHolder;
            _bookmarkDataHolder = bookmarkDataHolder;
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

            var keywords = searchText
                .Split(" ")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var tags = keywords.Where(x => x.StartsWith("t:")).ToArray();
            var tagSearchValues = tags.Select(x => x[2..]).ToArray();
            keywords = keywords.Except(tags).ToArray();

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

                result.AddScore(ScoreReason.ClickCount, item.ClickedCount);

                result.AddScore(ScoreReason.Title, keywords.Any(x => StringContains(item.Title, x)));

                result.AddScore(ScoreReason.TitleAlias, item.TitleAlias?.Values != null &&
                                                        item.TitleAlias.Values.Any(al =>
                                                            keywords.Any(x => StringContains(al, x))));

                result.AddScore(ScoreReason.Url, keywords.Any(x => StringContains(item.Url, x)));
                if (item.Tags?.Any() == true)
                {
                    foreach (var x in keywords)
                    {
                        var matched = item.Tags.Keys.Any(key => StringContains(key, x)) ||
                                      item.Tags.Values.Any(t =>
                                          t.TagAlias?.Values.Any(ta =>
                                              StringContains(ta, x)) == true);
                        if (matched)
                        {
                            result.AddScore(ScoreReason.TagAlias, true);
                            break;
                        }
                    }


                    result.AddScore(ScoreReason.Tags, tagSearchValues.Any() &&
                                                      item.Tags.Keys.Any(tag =>
                                                          tagSearchValues.Contains(tag,
                                                              StringComparer.OrdinalIgnoreCase)));
                }

                return result;
            }

            static bool StringContains(string a, string target)
            {
                return a.Contains(target, StringComparison.OrdinalIgnoreCase);
            }
        }

        private Func<Bk, bool> FilterByNode()
        {
            return bks => _bookmarkDataHolder.Nodes.ContainsKey(bks.Url);
        }
    }
}