using System.Text.Json;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services
{
    public class RecordService : IRecordService
    {
        private readonly IIndexedDbRepo<SearchRecord, long> _searchRecordRepo;
        private readonly IClock _clock;

        public RecordService(
            IIndexedDbRepo<SearchRecord, long> searchRecordRepo,
            IClock clock)
        {
            _searchRecordRepo = searchRecordRepo;
            _clock = clock;
        }

        public async Task AddAsync(UserClickRecord userClickRecord)
        {
            await _searchRecordRepo.UpsertAsync(new SearchRecord
            {
                Id = _clock.UtcNow,
                RecordJson = JsonSerializer.Serialize(userClickRecord)
            });
        }
    }
}