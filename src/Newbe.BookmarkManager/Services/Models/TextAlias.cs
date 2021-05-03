namespace Newbe.BookmarkManager.Services
{
    public record TextAlias
    {
        public TextAliasType TextAliasType { get; set; }
        public string Alias { get; set; }
        public long LastUpdatedTime { get; set; }
    }
}