using System.Linq;

namespace Newbe.BookmarkManager.Services
{
    public record SearchInput
    {
        public static SearchInput Parse(string searchText)
        {
            var searchInput = new SearchInput
            {
                SourceText = searchText
            };
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return searchInput;
            }

            var keywords = searchText
                .Split(" ")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var tags = keywords.Where(x => x.StartsWith("t:")).ToArray();
            var tagSearchValues = tags.Select(x => x[2..]).ToArray();
            keywords = keywords.Except(tags).ToArray();
            searchInput.Keywords = keywords;
            searchInput.Tags = tagSearchValues;
            return searchInput;
        }

        public string SourceText { get; set; }
        public string[] Keywords { get; set; }
        public string[] Tags { get; set; }
    }
}