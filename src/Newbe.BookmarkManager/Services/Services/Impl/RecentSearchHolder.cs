using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class RecentSearchHolder : IRecentSearchHolder
    {
        private readonly IIndexedDbRepo<RecentSearch, string> _recentSearchRepo;
        private readonly IClock _clock;

        public RecentSearch RecentSearch { get; private set; } = new()
        {
            Items = new List<RecentSearchItem>()
        };

        public RecentSearchHolder(
            IIndexedDbRepo<RecentSearch, string> recentSearchRepo,
            IClock clock)
        {
            _recentSearchRepo = recentSearchRepo;
            _clock = clock;
        }

        public async Task AddAsync(string text)
        {
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

            await _recentSearchRepo.UpsertAsync(RecentSearch);
        }

        public async Task<RecentSearch> LoadAsync()
        {
            RecentSearch = (await _recentSearchRepo.GetSingleOneAsync())!;
            RecentSearch ??= new RecentSearch
            {
                Items = new List<RecentSearchItem>()
            };
            return RecentSearch;
        }
    }
}