using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public class SmallCache : ISmallCache
    {
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
            Cache.Remove(key, out _);
        }
    }
}