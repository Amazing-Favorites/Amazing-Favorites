using System;

namespace Newbe.BookmarkManager.Services
{
    public record AfCodeResult
    {
        public string Url { get; init; } = null!;
        public string Title { get; init; } = null!;
        public string[] Tags { get; init; } = Array.Empty<string>();
        public AfCodeType AfCodeType { get; init; }
    }
}