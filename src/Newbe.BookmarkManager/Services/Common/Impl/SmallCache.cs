using System.Collections.Concurrent;
using System.Collections.Generic;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Services
{
    public class SmallCache : ISmallCache
    {
        private readonly IAfEventHub _afEventHub;

        public SmallCache(
            IAfEventHub afEventHub)
        {
            _afEventHub = afEventHub;
        }

        internal readonly ConcurrentDictionary<string, object> Cache = new();

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (Cache.TryGetValue(key, out var obj))
            {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }

        public void Set<T>(string key, T value)
        {
            Cache[key] = value!;
        }

        public void Remove(string key)
        {
            if (Cache.Remove(key, out _))
            {
                _afEventHub.PublishAsync(new SmallCacheExpiredEvent
                {
                    CacheKey = key
                });
            }
        }
    }
}