using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;

namespace Newbe.BookmarkManager.Services
{
    public class OneDriveClient : IOneDriveClient
    {
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<OneDriveClient> _logger;
        private readonly IOptions<OneDriveOAuthOptions> _oneDriveOAuthOptions;
        private readonly IIdentityApi _identityApi;

        public OneDriveClient(
            GraphServiceClient graphServiceClient,
            ILogger<OneDriveClient> logger,
            IOptions<OneDriveOAuthOptions> oneDriveOAuthOptions,
            IIdentityApi identityApi)
        {
            _graphClient = graphServiceClient;
            _logger = logger;
            _oneDriveOAuthOptions = oneDriveOAuthOptions;
            _identityApi = identityApi;
        }

        private static string? _authUrl = null;

        public async Task<bool> LoginAsync(bool interactive)
        {
            try
            {
                await LoginCoreAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to login Google Drive");
                if (interactive)
                {
                    throw;
                }
            }

            return false;


            async Task LoginCoreAsync()
            {
                var options = _oneDriveOAuthOptions.Value;
                var redirectUrl = await _identityApi.GetRedirectURL("");
                if (_authUrl == null)
                {
                    _authUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
                    var clientId = options.Type == OAuth2ClientType.Dev
                        ? options.DevClientId
                        : options.ClientId;
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=token";
                    _authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(" ", options.DefaultScopes))}";
                }

                var callbackUrl = await _identityApi.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
                {
                    Interactive = interactive,
                    Url = new HttpURL(_authUrl)
                });
                var token = callbackUrl.Split("#")[1].Split("&")[0].Split("=")[1];

                Console.WriteLine(token);
                StaticAuthProvider.Token = token;
                _logger.LogInformation("One Drive login success");
            }
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

        public async Task<DriveItem> UploadingFileAsync(Stream fileStream, string itemPath)
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
                    new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxSliceSize, _graphClient);
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
}