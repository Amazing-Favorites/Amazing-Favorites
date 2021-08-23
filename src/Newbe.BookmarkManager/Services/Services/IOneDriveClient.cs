
using Microsoft.Graph;

namespace Newbe.BookmarkManager.Services.Services;
public interface IOneDriveClient
{
    Task<User> GetMeAsync();
    Task<Drive> GetOneDriveAsync();

    Task<IEnumerable<DriveItem>> GetDriveContentsAsync();

    Task<Stream> GetFileStreamByItemId(string id);

    Task<IEnumerable<DriveItem>> SearchFileFromDriveAsync(string searchString);

    Task<DriveItem> UploadingFileAsync(Stream fileStream, string itemPath);
}
