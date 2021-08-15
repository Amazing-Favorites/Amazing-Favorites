using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services
{
    public class SyncAliasJob : ISyncAliasJob
    {
        private readonly ILogger<SyncAliasJob> _logger;
        private readonly IClock _clock;
        private readonly IIndexedDbRepo<BkTag, string> _tagsRepo;
        private readonly IIndexedDbRepo<Bk, string> _bkRepo;
        private readonly IUserOptionsService _userOptionsService;
        private readonly IEnumerable<ITextAliasProvider> _fillers;
        private readonly Subject<long> _jobSubject = new();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _loadHandler;

        public SyncAliasJob(
            ILogger<SyncAliasJob> logger,
            IClock clock,
            IIndexedDbRepo<BkTag, string> tagsRepo,
            IIndexedDbRepo<Bk, string> bkRepo,
            IUserOptionsService userOptionsService,
            IEnumerable<ITextAliasProvider> fillers)
        {
            _logger = logger;
            _clock = clock;
            _tagsRepo = tagsRepo;
            _bkRepo = bkRepo;
            _userOptionsService = userOptionsService;
            _fillers = fillers;
        }

        public async ValueTask StartAsync()
        {
            _loadHandler = _jobSubject
                .Merge(Observable.Interval(TimeSpan.FromSeconds(60)))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        var options = await _userOptionsService.GetOptionsAsync();
                        if (options is
                            {
                                AcceptPrivacyAgreement: true,
                                PinyinFeature:
                                {
                                    Enabled: true,
                                    AccessToken: not null,
                                    BaseUrl: not null
                                }
                            })
                        {
                            foreach (var textAliasFiller in _fillers)
                            {
                                var tags = await _tagsRepo.GetAllAsync();
                                var bkTags = tags
                                    .Where(tag => textAliasFiller.NeedFill(tag))
                                    .ToArray();
                                if (bkTags.Length > 0)
                                {
                                    var updateResult = await textAliasFiller.FillAsync(bkTags);
                                    if (updateResult.IsOk)
                                    {
                                        foreach (var bkTag in bkTags)
                                        {
                                            await _tagsRepo.UpsertAsync(bkTag);
                                        }

                                        _logger.LogInformation("New alias add for tag");
                                    }
                                }
                            }

                            foreach (var textAliasFiller in _fillers)
                            {
                                var bkCollection = await _bkRepo.GetAllAsync();
                                var bks = bkCollection
                                    .Where(x => textAliasFiller.NeedFill(x))
                                    .ToArray();
                                if (bks.Length > 0)
                                {
                                    _logger.LogInformation("Found {Count} bks need to add alias in {TextAliasType}",
                                        bks.Length,
                                        textAliasFiller.TextAliasType);
                                    const int pageSize = 100;
                                    var pageCount = (int)Math.Ceiling(1.0 * bks.Length / pageSize);
                                    for (var i = 0; i < pageCount; i++)
                                    {
                                        var items = bks.Skip(i * pageSize).Take(pageSize).ToArray();
                                        var updateResult = await textAliasFiller.FillAsync(items);
                                        if (updateResult.IsOk)
                                        {
                                            foreach (var item in items)
                                            {
                                                await _bkRepo.UpsertAsync(item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to fill alias");
                    }
                }))
                .Concat()
                .Subscribe(_ => { });
            _jobSubject.OnNext(_clock.UtcNow);
        }
    }
}