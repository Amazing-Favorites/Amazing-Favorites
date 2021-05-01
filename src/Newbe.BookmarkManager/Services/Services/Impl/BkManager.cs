using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class BkManager : IBkManager
    {
        private readonly IBkDataHolder _bkDataHolder;
        private readonly IBookmarkDataHolder _bookmarkDataHolder;
        private readonly IClock _clock;
        private readonly ILogger<BkManager> _logger;

        public BkManager(
            ILogger<BkManager> logger,
            IBkDataHolder bkDataHolder,
            IBookmarkDataHolder bookmarkDataHolder,
            IClock clock)
        {
            _bkDataHolder = bkDataHolder;
            _bookmarkDataHolder = bookmarkDataHolder;
            _clock = clock;
            _logger = logger;
        }

        public async ValueTask RemoveTagAsync(string url, string tag)
        {
            var bkEntityCollection = _bkDataHolder.Collection;
            if (bkEntityCollection.Bks.TryGetValue(url, out var bk))
            {
                if (bk.Tags?.ContainsKey(tag) == true)
                {
                    bk.Tags.Remove(tag);
                    _logger.LogInformation("Tag {Tag} removed from {Url}", tag, url);
                }

                await _bkDataHolder.MarkUpdatedAsync();
            }
        }

        public async ValueTask<bool> AddTagAsync(string url, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            _logger.LogInformation("New tag {Tag} add to {Url}", tag, url);
            var bkEntityCollection = _bkDataHolder.Collection;
            if (bkEntityCollection.Bks.TryGetValue(url, out var bk))
            {
                bk.Tags ??= new();
                var key = tag.Trim();
                if (bk.Tags.ContainsKey(key))
                {
                    return false;
                }

                bk.Tags[key] = new BkTag
                {
                    Tag = key,
                    TagAlias = new Dictionary<BkAliasType, string>()
                };
                _logger.LogInformation("Tag {Tag} added for {Url}", tag, url);
            }

            await _bkDataHolder.MarkUpdatedAsync();
            return true;
        }

        public async ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags)
        {
            var bkEntityCollection = _bkDataHolder.Collection;
            if (bkEntityCollection.Bks.TryGetValue(url, out var bk))
            {
                bk.Tags = tags.ToDictionary(x => x, x => new BkTag
                {
                    Tag = x,
                    TagAlias = new Dictionary<BkAliasType, string>()
                });

                _logger.LogInformation("Tag {Tags} added for {Url}", tags, url);
                await _bkDataHolder.SaveNowAsync();
            }
        }

        public async ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls)
        {
            var updatedCount = 0;
            foreach (var (url, furl) in urls)
            {
                var bkEntityCollection = _bkDataHolder.Collection;
                if (bkEntityCollection.Bks.TryGetValue(url, out var bk) &&
                    bk.FavIconUrl != furl)
                {
                    bk.FavIconUrl = furl;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                _logger.LogInformation("There are {Count} favicon url updated", updatedCount);
                await _bkDataHolder.MarkUpdatedAsync();
            }
        }

        public Bk Get(string url)
        {
            var bkEntityCollection = _bkDataHolder.Collection;
            return bkEntityCollection.Bks.TryGetValue(url, out var bk) ? bk : null;
        }

        public async ValueTask AddClickAsync(string url, int moreCount)
        {
            var bkEntityCollection = _bkDataHolder.Collection;
            if (bkEntityCollection.Bks.TryGetValue(url, out var bk))
            {
                bk.ClickedCount += moreCount;
                bk.LastClickTime = _clock.UtcNow;
            }

            await _bkDataHolder.MarkUpdatedAsync();
        }

        public async ValueTask RestoreAsync()
        {
            await _bkDataHolder.RestoreAsync();
        }
    }
}