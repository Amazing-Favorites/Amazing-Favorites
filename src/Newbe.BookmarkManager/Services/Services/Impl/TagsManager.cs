using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class TagsManager : ITagsManager
    {
        private readonly ILogger<TagsManager> _logger;
        private readonly IIndexedDbRepo<BkTag, string> _tagsRepo;
        private readonly IClock _clock;
        private readonly IUserOptionsService _userOptionsService;

        public TagsManager(
            ILogger<TagsManager> logger,
            IIndexedDbRepo<BkTag, string> tagsRepo,
            IClock clock,
            IUserOptionsService userOptionsService)
        {
            _logger = logger;
            _tagsRepo = tagsRepo;
            _clock = clock;
            _userOptionsService = userOptionsService;
        }

        public async Task<List<BkTag>> GetHotAsync()
        {
            var sw = Stopwatch.StartNew();
            var re = await GetHotAsyncCore();
            _logger.LogInformation("Get Hot Tags cost {Time} ms", sw.ElapsedMilliseconds);
            return re;

            async Task<List<BkTag>> GetHotAsyncCore()
            {
                var options = await _userOptionsService.GetOptionsAsync();
                var all = await _tagsRepo.GetAllAsync();
                var result = all
                    .Where(x => x.RelatedBkCount > 0)
                    .OrderByDescending(x => x.ClickedCount)
                    .ThenByDescending(x => x.LastClickTime)
                    .ThenByDescending(x => x.RelatedBkCount)
                    .Take(options.HotTagsFeature?.ListCount ?? 10)
                    .ToList();
                return result;
            }
        }

        public async Task AddCountAsync(string tag, int count)
        {
            var bkTag = await _tagsRepo.GetAsync(tag);
            if (bkTag != null)
            {
                bkTag.ClickedCount += count;
                bkTag.LastClickTime = _clock.UtcNow;
                await _tagsRepo.UpsertAsync(bkTag);
            }
        }

        public async Task<string[]> GetAllTagsAsync()
        {
            var tags = await _tagsRepo.GetAllAsync();
            var re = tags.Select(x => x.Tag).OrderBy(x => x).ToArray();
            return re;
        }

        public async Task UpdateRelatedCountAsync(Dictionary<string, int> counts)
        {
            var tags = await _tagsRepo.GetAllAsync();
            if (tags.Count == 0)
            {
                return;
            }

            var tagDict = tags.ToDictionary(x => x.Tag);
            var updated = new List<BkTag>(tags.Count);
            foreach (var (tag, count) in counts)
            {
                if (tagDict.TryGetValue(tag, out var bkTag))
                {
                    bkTag.RelatedBkCount = count;
                    updated.Add(bkTag);
                }
            }

            if (updated.Count > 0)
            {
                foreach (var bkTag in updated)
                {
                    await _tagsRepo.UpsertAsync(bkTag);
                }
            }
        }
    }
}