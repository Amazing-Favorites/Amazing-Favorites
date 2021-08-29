using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Services
{
    public class RecentSearchHolder : IRecentSearchHolder
    {
        private readonly ISimpleDataStorage _simpleDataStorage;
        private readonly IClock _clock;

        public RecentSearch RecentSearch { get; private set; } = new()
        {
            Items = new List<RecentSearchItem>()
        };

        public RecentSearchHolder(
            ISimpleDataStorage simpleDataStorage,
            IClock clock)
        {
            _simpleDataStorage = simpleDataStorage;
            _clock = clock;
        }

        public async Task AddAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var items = RecentSearch.Items;
            var item = items.Find(item => item.Text == text);
            if (item == null)
            {
                item = new RecentSearchItem
                {
                    Text = text
                };
            }
            else
            {
                items.Remove(item);
            }

            item.LastTime = _clock.UtcNow;
            item.SearchCount++;
            items.Insert(0, item);
            const int maxCount = 10;
            while (items.Count > maxCount)
            {
                items.RemoveAt(maxCount - 1);
            }

            await _simpleDataStorage.SaveAsync(RecentSearch);
        }

        public async Task<RecentSearch> LoadAsync()
        {
            RecentSearch = await _simpleDataStorage.GetOrDefaultAsync<RecentSearch>();
            return RecentSearch;
        }
    }
}