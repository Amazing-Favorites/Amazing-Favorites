using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public record PinyinOutput
    {
        public bool IsOk { get; set; }
        public Dictionary<string, string> Pinyin { get; set; }
    }

    public record PinyinInput
    {
        public string[] Text { get; set; }
    }
}