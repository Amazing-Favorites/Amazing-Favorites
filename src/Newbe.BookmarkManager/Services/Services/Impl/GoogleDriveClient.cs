using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.HighPerformance;
using Newbe.BookmarkManager.WebApi;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;
using File = Google.Apis.Drive.v3.Data.File;

namespace Newbe.BookmarkManager.Services
{
    public class GoogleDriveClient : IGoogleDriveClient
    {
        public const string DataFileName = "af.data.json";
        private readonly ILogger<GoogleDriveClient> _logger;
        private readonly IIdentityApi _identityApi;
        private readonly IOptions<GoogleDriveOAuthOptions> _googleDriveOauthOptions;
        private DriveService? _driveService;
        private string? _fileId;

        public GoogleDriveClient(
            ILogger<GoogleDriveClient> logger,
            IIdentityApi identityApi,
            IOptions<GoogleDriveOAuthOptions> googleDriveOauthOptions)
        {
            _logger = logger;
            _identityApi = identityApi;
            _googleDriveOauthOptions = googleDriveOauthOptions;
        }

        private static string? _authUrl = null;

        public void LoadToken(string token)
        {
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = new TokenBaseInitializer(token),
                GZipEnabled = false,
            });
        }

        public async Task<string?> LoginAsync(bool interactive)
        {
            try
            {
                return await LoginCoreAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "failed to login Google Drive");
                if (interactive)
                {
                    throw;
                }
            }

            return default;

            async Task<string?> LoginCoreAsync()
            {
                var options = _googleDriveOauthOptions.Value;
                var redirectUrl = await _identityApi.GetRedirectURL("");
                if (_authUrl == null)
                {
                    _authUrl = "https://accounts.google.com/o/oauth2/auth";
                    var clientId = options.Type == OAuth2ClientType.Dev
                        ? options.DevClientId
                        : options.ClientId;
                    _authUrl += $"?client_id={clientId}";
                    _authUrl += "&response_type=token";
                    _authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
                    _authUrl += $"&scope={WebUtility.UrlEncode(string.Join(" ", options.Scopes))}";
                }

                var callbackUrl = await _identityApi.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
                {
                    Interactive = interactive,
                    Url = new HttpURL(_authUrl)
                });
                var token = callbackUrl.Split("#")[1].Split("&")[0].Split("=")[1];
                LoadToken(token);
                _logger.LogInformation("Google Drive login success");
                return token;
            }
        }

        public async Task<CloudDataDescription?> GetFileDescriptionAsync()
        {
            Debug.Assert(_driveService != null, nameof(_driveService) + " != null");
            var listRequest = _driveService.Files.List();
            listRequest.Spaces = "appDataFolder";
            listRequest.Fields = "files(id, name, description)";
            var fileList = await listRequest.ExecuteAsync();
            var file = fileList.Files.FirstOrDefault(x => x.Name == DataFileName);
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

            Debug.Assert(_driveService != null, nameof(_driveService) + " != null");
            var getRequest = _driveService.Files.Get(_fileId);
            await using var memoryStream = new MemoryStream();
            await getRequest.DownloadAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var re = await JsonSerializer.DeserializeAsync<CloudBkCollection>(memoryStream);
            return re;
        }

        public async Task UploadAsync(CloudBkCollection cloudBkCollection)
        {
            Debug.Assert(_driveService != null, nameof(_driveService) + " != null");
            var file = new File()
            {
                Name = DataFileName,
                Description = JsonSerializer.Serialize(new CloudDataDescription
                {
                    EtagVersion = cloudBkCollection.EtagVersion,
                    LastUpdateTime = cloudBkCollection.LastUpdateTime
                })
            };
            await using var stream = JsonSerializer.SerializeToUtf8Bytes(cloudBkCollection).AsMemory().AsStream();
            if (_fileId == null)
            {
                file.Parents = new List<string>
                {
                    "appDataFolder"
                };
                var updateRequest = _driveService.Files.Create(file, stream, "application/json");
                await updateRequest.UploadAsync();
                _fileId = updateRequest.ResponseBody.Id;
            }
            else
            {
                var updateRequest = _driveService.Files.Update(file, _fileId, stream, "application/json");
                await updateRequest.UploadAsync();
            }
        }

        public async Task<bool> TestAsync()
        {
            try
            {
                return await TestCoreAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error when google drive test");
                return false;
            }

            async Task<bool> TestCoreAsync()
            {
                if (_driveService == null)
                {
                    return false;
                }

                try
                {
                    var listRequest = _driveService.Files.List();
                    listRequest.Spaces = "appDataFolder";
                    await listRequest.ExecuteAsync();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed to test Google Drive api");
                }

                return false;
            }
        }
    }
}