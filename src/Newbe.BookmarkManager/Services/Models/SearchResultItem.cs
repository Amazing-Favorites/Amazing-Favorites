using System.Collections.Generic;
using System.Linq;

namespace Newbe.BookmarkManager.Services
{
    public record SearchResultItem(Bk Bk)
    {
        public int ClickCount { get; init; }

        public long LastClickTime { get; init; }
        public bool Matched => Score > 0;
        public double Score => MatchedDetails.Values.Sum();
        public Dictionary<ScoreReason, int> MatchedDetails { get; set; } = new();

        private static readonly Dictionary<ScoreReason, int> ScoreDictionary = new()
        {
            { ScoreReason.Title, 100 },
            { ScoreReason.TitleAlias, 80 },
            { ScoreReason.Url, 50 },
            { ScoreReason.Tags, 20 },
            { ScoreReason.TagAlias, 20 },
        };

        public void AddScore(ScoreReason reason, bool matched)
        {
            if (!MatchedDetails.TryGetValue(reason, out var now))
            {
                now = 0;
            }

            now += matched ? ScoreDictionary[reason] : 0;
            MatchedDetails[reason] = now;
        }

        public void AddScore(ScoreReason reason, int score)
        {
            if (!MatchedDetails.TryGetValue(reason, out var now))
            {
                now = 0;
            }

            now += score;

            MatchedDetails[reason] = now;
        }
    }

    public enum ScoreReason
    {
        Const,
        ClickCount,
        Title,
        TitleAlias,
        Url,
        Tags,
        TagAlias
    }
}