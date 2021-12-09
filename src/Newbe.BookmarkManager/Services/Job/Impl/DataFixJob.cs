using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services;

public class DataFixJob : IDataFixJob
{
    private readonly IUrlHashService _urlHashService;
    private readonly IIndexedDbRepo<Bk, string> _bkRepo;

    public DataFixJob(
        IUrlHashService urlHashService,
        IIndexedDbRepo<Bk, string> bkRepo)
    {
        _urlHashService = urlHashService;
        _bkRepo = bkRepo;
    }

    public async ValueTask StartAsync()
    {
        var list = await _bkRepo.GetAllAsync();
        foreach (var bk in list)
        {
            if (string.IsNullOrEmpty(bk.UrlHash))
            {
                bk.UrlHash = _urlHashService.GetHash(bk.Url);
                await _bkRepo.UpsertAsync(bk);
            }
        }
    }
}