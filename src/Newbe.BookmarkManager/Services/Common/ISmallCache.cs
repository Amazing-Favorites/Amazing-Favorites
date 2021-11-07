﻿namespace Newbe.BookmarkManager.Services
{
    public interface ISmallCache
    {
        bool TryGetValue<T>(string key, out T? value);
        void Set<T>(string key, T value);
        void Remove(string key);
    }
}