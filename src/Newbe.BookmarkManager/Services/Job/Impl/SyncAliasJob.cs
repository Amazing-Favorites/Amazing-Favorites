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
        private readonly IBkDataHolder _bkDataHolder;
        private readonly IClock _clock;
        private readonly IUserOptionsRepository _userOptionsRepository;
        private readonly IEnumerable<ITextAliasProvider> _fillers;
        private readonly Subject<long> _jobSubject = new();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable _loadHandler;

        public SyncAliasJob(
            ILogger<SyncAliasJob> logger,
            IBkDataHolder bkDataHolder,
            IClock clock,
            IUserOptionsRepository userOptionsRepository,
            IEnumerable<ITextAliasProvider> fillers)
        {
            _logger = logger;
            _bkDataHolder = bkDataHolder;
            _clock = clock;
            _userOptionsRepository = userOptionsRepository;
            _fillers = fillers;
        }

        public async ValueTask StartAsync()
        {
            await _bkDataHolder.StartAsync();
            _loadHandler = _jobSubject
                .Merge(Observable.Interval(TimeSpan.FromSeconds(60)))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    try
                    {
                        var options = await _userOptionsRepository.GetOptionsAsync();
                        if (options?.PinyinFeature is
                        {
                            Enabled: true,
                            AccessToken: not null,
                            BaseUrl: not null
                        })
                        {
                            foreach (var textAliasFiller in _fillers)
                            {
                                var bkTags = _bkDataHolder.Collection.Tags.Values
                                    .Where(tag => textAliasFiller.NeedFill(tag))
                                    .ToArray();
                                if (bkTags.Length > 0)
                                {
                                    await _bkDataHolder.PushDataChangeActionAsync(async () =>
                                    {
                                        var updateResult = await textAliasFiller.FillAsync(bkTags);
                                        if (updateResult.IsOk)
                                        {
                                            _logger.LogInformation("New alias add for tag");
                                        }

                                        return updateResult.IsOk;
                                    });
                                }
                            }

                            foreach (var textAliasFiller in _fillers)
                            {
                                var bks = _bkDataHolder.Collection.Bks.Values
                                    .Where(x => textAliasFiller.NeedFill(x))
                                    .ToArray();
                                if (bks.Length > 0)
                                {
                                    _logger.LogInformation("Found {Count} bks need to add alias in {TextAliasType}",
                                        bks.Length,
                                        textAliasFiller.TextAliasType);
                                    const int pageSize = 100;
                                    var pageCount = (int) Math.Ceiling(1.0 * bks.Length / pageSize);
                                    await _bkDataHolder.PushDataChangeActionAsync(async () =>
                                    {
                                        var updated = false;
                                        for (var i = 0; i < pageCount; i++)
                                        {
                                            var items = bks.Skip(i * pageSize).Take(pageSize).ToArray();
                                            var updateResult = await textAliasFiller.FillAsync(items);
                                            updated = updated || updateResult.IsOk;
                                        }

                                        return updated;
                                    });
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