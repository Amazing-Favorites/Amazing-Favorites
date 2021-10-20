using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Toolkit.HighPerformance;
using Newbe.BookmarkManager.WebApi;
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
        private string? _fileId;

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

        public void LoadToken(string token)
        {
            StaticAuthProvider.Token = token;
        }

        public async Task<string?> LoginAsync(bool interactive)
        {
            try
            {
                return await LoginCoreAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to login One Drive");
                if (interactive)
                {
                    throw;
                }
            }

            return default;

            async Task<string?> LoginCoreAsync()
            {
                var options = _oneDriveOAuthOptions.Value;
                var redirectUrl = await _identityApi.GetRedirectURL("");
                if (_authUrl == null)
                {
                    _authUrl = options.Authority;
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

                LoadToken(token);
                _logger.LogInformation("OneDrive login success");
                return token;
            }
        }

        public async Task<CloudDataDescription?> GetFileDescriptionAsync()
        {
            var file = await _graphClient.Me.Drive.Special.AppRoot
                .ItemWithPath(Consts.Cloud.CloudDataFileName)
                .Request()
                .GetAsync();
            if (file == null)
            {
                return null;
            }

            _fileId = file.Id;
            var cloudDataDescription = await JsonHelper.DeserializeAsync<CloudDataDescription>(file.Description);
            return cloudDataDescription;
        }

        public async Task<CloudBkCollection?> GetCloudDataAsync()
        {
            if (_fileId == null)
            {
                return default;
            }

            await using var stream = await _graphClient.Me.Drive.Items[_fileId]
                .Content
                .Request()
                .GetAsync();
            var re = await JsonSerializer.DeserializeAsync<CloudBkCollection>(stream);
            return re;
        }

        public async Task UploadAsync(CloudBkCollection cloudBkCollection)
        {
            var uploadProps = new DriveItemUploadableProperties
            {
                ODataType = null,
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "replace" }
                },
                Name = Consts.Cloud.CloudDataFileName,
                Description = JsonSerializer.Serialize(new CloudDataDescription
                {
                    EtagVersion = cloudBkCollection.EtagVersion,
                    LastUpdateTime = cloudBkCollection.LastUpdateTime
                })
            };
            await using var stream = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().AsStream();
            var uploadSession = await _graphClient.Me.Drive.Special.AppRoot
                .ItemWithPath(Consts.Cloud.CloudDataFileName)
                .CreateUploadSession(uploadProps)
                .Request()
                .PostAsync();
            const int maxSliceSize = 320 * 1024;
            var fileUploadTask =
                new LargeFileUploadTask<DriveItem>(uploadSession, stream, maxSliceSize, _graphClient);
            var uploadResult = await fileUploadTask.UploadAsync();
            _fileId = uploadResult.ItemResponse.Id;
        }

        public Task<bool> TestAsync()
        {
            try
            {
                return Task.FromResult(TestCoreAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error when One Drive test");
                return Task.FromResult(false);
            }

            bool TestCoreAsync()
            {
                try
                {
                    _graphClient.Me.Drive
                        .Request()
                        .GetAsync();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed to test One Drive api");
                }

                return false;
            }
        }
    }
}