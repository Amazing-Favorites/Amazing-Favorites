using System;

namespace Newbe.BookmarkManager.Services;

class SystemClock : IClock
{
    public long UtcNow => DateTimeOffset.Now.ToUnixTimeSeconds();
}