using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newbe.BookmarkManager.WebApi;
using WebExtensions.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public class IndexedBkManager : IBkManager
    {
        private readonly ILogger<IndexedBkManager> _logger;
        private readonly IClock _clock;
        private readonly IUrlHashService _urlHashService;
        private readonly IIndexedDbRepo<Bk, string> _bkRepo;
        private readonly IIndexedDbRepo<BkMetadata, string> _bkMetadataRepo;
        private readonly IIndexedDbRepo<BkTag, string> _tagsRepo;

        public IndexedBkManager(
            ILogger<IndexedBkManager> logger,
            IClock clock,
            IUrlHashService urlHashService,
            IIndexedDbRepo<Bk, string> bkRepo,
            IIndexedDbRepo<BkMetadata, string> bkMetadataRepo,
            IIndexedDbRepo<BkTag, string> tagsRepo)
        {
            _logger = logger;
            _clock = clock;
            _urlHashService = urlHashService;
            _bkRepo = bkRepo;
            _bkMetadataRepo = bkMetadataRepo;
            _bkRepo = bkRepo;
            _tagsRepo = tagsRepo;
        }

        public async ValueTask AddClickAsync(string url, int moreCount)
        {
            var bk = await _bkRepo.GetAsync(url);
            if (bk == null)
            {
                return;
            }

            bk.ClickedCount += moreCount;
            await _bkRepo.UpsertAsync(bk);
        }

        public async ValueTask RestoreAsync()
        {
            await _bkRepo.DeleteAllAsync();
            await _bkMetadataRepo.UpsertAsync(new BkMetadata
            {
                Id = Consts.SingleOneDataId
            });
        }

        public async ValueTask RemoveTagAsync(string url, string tag)
        {
            var bk = await _bkRepo.GetAsync(url);
            if (bk != null)
            {
                if (bk.Tags?.Contains(tag) == true)
                {
                    bk.Tags.Remove(tag);
                    await _bkRepo.UpsertAsync(bk);
                    await UpdateMetadataAsync();
                    _logger.LogInformation("Tag {Tag} removed from {Url}", tag, url);
                }
            }
        }

        public async ValueTask<bool> AddTagAsync(string url, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            _logger.LogInformation("New tag {Tag} add to {Url}", tag, url);
            var bk = await _bkRepo.GetAsync(url);
            if (bk != null)
            {
                var key = tag.Trim();
                if (bk.Tags != null && bk.Tags.Contains(key))
                {
                    return false;
                }

                bk.Tags ??= new List<string>();
                bk.Tags.Add(key);
                await AppendTagsAsync(key);
                await _bkRepo.UpsertAsync(bk);
                await UpdateMetadataAsync();
                _logger.LogInformation("Tag {Tag} added for {Url}", key, url);
            }

            return true;
        }

        public async ValueTask UpdateTagsAsync(string url, IEnumerable<string> tags)
        {
            var bk = await _bkRepo.GetAsync(url);
            if (bk != null)
            {
                var tagList = tags.ToList();
                foreach (var tag in tagList)
                {
                    await AppendTagsAsync(tag);
                }

                bk.Tags = tagList;
                await _bkRepo.UpsertAsync(bk);
                await UpdateMetadataAsync();
                _logger.LogInformation("Tag {Tags} added for {Url}", tagList, url);
            }
        }

        private async Task AppendTagsAsync(string tag)
        {
            var oldTag = await _tagsRepo.GetAsync(tag);
            if (oldTag == null)
            {
                _logger.LogInformation("A new tag {Tag} added to all collection", oldTag);
                oldTag = new BkTag
                {
                    Tag = tag,
                    TagAlias = new()
                };
                await _tagsRepo.UpsertAsync(oldTag);
            }
        }

        public async ValueTask UpdateFavIconUrlAsync(Dictionary<string, string> urls)
        {
            foreach (var (url, furl) in urls)
            {
                var bk = await _bkRepo.GetAsync(url);
                if (bk != null && bk.FavIconUrl != furl)
                {
                    bk.FavIconUrl = furl;
                    await _bkRepo.UpsertAsync(bk);
                    _logger.LogInformation("FavIconUrl: {FavIconUrl} updated for url: {Url}", furl, bk.Url);
                }
            }
        }

        public async ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> nodes)
        {
            var updated = false;
            var bkDic = nodes.ToLookup(x => x.Url, x => new Bk
            {
                Title = x.Title,
                TitleLastUpdateTime = _clock.UtcNow,
                Url = x.Url,
                UrlHash = _urlHashService.GetHash(x.Url),
                LastCreateTime = x.DateAdded.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long) x.DateAdded.Value).ToUnixTimeSeconds()
                    : 0L
            });
            var bookmarksKeys = bkDic.Select(x => x.Key).ToHashSet();
            _logger.LogDebug("Found {Count} bookmark", bookmarksKeys.Count);

            foreach (var grouping in bkDic)
            {
                var url = grouping.Key;
                var node = grouping.First();
                var bk = await _bkRepo.GetAsync(url);
                if (bk != null)
                {
                    if (bk.Title != node.Title)
                    {
                        bk.Title = node.Title;
                        bk.TitleLastUpdateTime = _clock.UtcNow;
                        await _bkRepo.UpsertAsync(bk);
                        updated = true;
                    }
                }
            }

            var bks = await _bkRepo.GetAllAsync();
            var newKeys = bookmarksKeys.Except(bks.Select(x => x.Url)).ToArray();

            if (newKeys.Length > 0)
            {
                foreach (var key in newKeys)
                {
                    var bk = bkDic[key].First();
                    await _bkRepo.UpsertAsync(bk);
                    updated = true;
                }

                _logger.LogInformation("There are {Count} links new, add to storage", newKeys.Length);
            }

            if (updated)
            {
                await UpdateMetadataAsync();
            }
        }

        public async Task LoadCloudCollectionAsync(CloudBkCollection cloudBkCollection)
        {
            var bks = await _bkRepo.GetAllAsync();
            var dictByUrlHash = bks.ToDictionary(x => x.UrlHash);
            foreach (var (urlHash, cloudBk) in cloudBkCollection.Bks)
            {
                if (dictByUrlHash.TryGetValue(urlHash, out var localBk))
                {
                    localBk.Tags = cloudBk.Tags;
                    await _bkRepo.UpsertAsync(localBk);
                }
            }

            var cloudTags = cloudBkCollection.Bks.SelectMany(x=>x.Value.Tags).ToHashSet();
            foreach (var cloudBkTag in cloudTags)
            {
                await AppendTagsAsync(cloudBkTag);
            }

            await _bkMetadataRepo.UpsertAsync(new BkMetadata
            {
                EtagVersion = cloudBkCollection.EtagVersion,
                LastUpdateTime = cloudBkCollection.LastUpdateTime,
                Id = Consts.SingleOneDataId
            });
        }

        public async Task<CloudBkCollection> GetCloudBkCollectionAsync()
        {
            var local = await _bkRepo.GetAllAsync();
            var meta = await GetMetadataAsync();
            var re = new CloudBkCollection
            {
                Bks = local
                    .Where(x => x.Tags?.Count > 0)
                    .ToDictionary(x => x.UrlHash, x => new CloudBk
                    {
                        Tags = x.Tags!,
                    }),
                LastUpdateTime = meta.LastUpdateTime,
                EtagVersion = meta.EtagVersion
            };
            return re;
        }

        public async Task DeleteAsync(string url)
        {
            await _bkRepo.DeleteAsync(url);
        }

        public async Task UpdateTitleAsync(string url, string title)
        {
            var bk = await _bkRepo.GetAsync(url);
            if (bk != null)
            {
                bk.Title = title;
                await _bkRepo.UpsertAsync(bk);
            }
        }

        public async Task<long> GetEtagVersionAsync()
        {
            var meta = await GetMetadataAsync();
            return meta.EtagVersion;
        }

        public async Task<Bk?> Get(string url)
        {
            var re = await _bkRepo.GetAsync(url);
            return re;
        }

        private async Task<BkMetadata> GetMetadataAsync()
        {
            var meta = await _bkMetadataRepo.GetSingleOneAsync() ?? new BkMetadata();
            return meta;
        }

        private async Task UpdateMetadataAsync()
        {
            var meta = await _bkMetadataRepo.GetSingleOneAsync() ?? new BkMetadata();
            meta.EtagVersion += 1;
            meta.LastUpdateTime = _clock.UtcNow;
            await _bkMetadataRepo.UpsertAsync(meta);
        }
    }
}