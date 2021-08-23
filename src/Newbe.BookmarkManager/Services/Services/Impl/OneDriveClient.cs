
using Microsoft.Graph;

namespace Newbe.BookmarkManager.Services.Services.Impl;
public class OneDriveClient : IOneDriveClient
{
    private GraphServiceClient _graphClient { get; set; }


    private ILogger<OneDriveClient> _logger;
    public OneDriveClient(
        GraphServiceClient graphServiceClient,
        ILogger<OneDriveClient> logger
        )
    {
        _graphClient = graphServiceClient;
        _logger = logger;
    }
    public async Task<IEnumerable<DriveItem>> GetDriveContentsAsync()
    {
        try
        {
            return await _graphClient.Me.Drive.Root.Children.Request().GetAsync();
        }
        catch (ServiceException ex)
        {

            return null;
        }
    }

    public async Task<Stream> GetFileStreamByItemId(string id)
    {
        try
        {
            return await _graphClient.Me.Drive.Items[id].Content.Request().GetAsync();
        }
        catch (ServiceException ex)
        {

            return null;
        }
    }

    public async Task<User> GetMeAsync()
    {
        try
        {
            var t1 = await _graphClient.Me.Request().GetAsync();

            _logger.LogInformation(t1.Id);
            return t1;
        }
        catch (ServiceException ex)
        {
            _logger.LogInformation(ex.Message);
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task<Drive> GetOneDriveAsync()
    {
        try
        {
            // GET /me
            return await _graphClient.Me.Drive.Request().GetAsync();
        }
        catch (ServiceException ex)
        {
            return null;
        }
    }

    public async Task<IEnumerable<DriveItem>> SearchFileFromDriveAsync(string searchKey)
    {
        try
        {
            var itemList = await _graphClient.Me.Drive.Root.Search("searchKey")
                .Request()
                .GetAsync();
            return itemList;
        }
        catch (ServiceException ex)
        {
            return null;
        }
    }
    public async Task<DriveItem> UploadingFileAsync(Stream fileStream,string itemPath)
    {
        try
        {
            var uploadProps = new DriveItemUploadableProperties
            {
                ODataType = null,
                AdditionalData = new Dictionary<string, object>
                    {
                        { "@microsoft.graph.conflictBehavior", "replace" }
                    }
            };
            var uploadSession = await _graphClient.Me
                .Drive.Root
                .ItemWithPath(itemPath)
                .CreateUploadSession(uploadProps)
                .Request()
                .PostAsync();
            int maxSliceSize = 320 * 1024;
            var fileUploadTask =
                new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxSliceSize,_graphClient);
            IProgress<long> progress = new Progress<long>(prog =>
            {
                Console.WriteLine($"Uploaded {prog} bytes of {fileStream.Length} bytes");
            });
            var uploadResult = await fileUploadTask.UploadAsync(progress);
            return uploadResult.ItemResponse;
        }
        catch (ServiceException ex)
        {
            return null;
        }
    }

}
