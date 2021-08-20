using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Microsoft.AspNetCore.Components.Web;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;
using File = Google.Apis.Drive.v3.Data.File;

namespace Newbe.BookmarkManager.Pages
{
    public partial class MyGoogle
    {
        public string Token { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task Callback(MouseEventArgs obj)
        {
            var redirectUrl = await WebExtensions.Identity.GetRedirectURL("");
            var clientID = "108132292975-3b9jr2udtr9i7ap8je5amtqmfefle0la.apps.googleusercontent.com";
            var scopes = new[]
            {
                "https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/drive.appdata",
                "https://www.googleapis.com/auth/drive.file",
                "https://www.googleapis.com/auth/drive.install",
            };
            var authUrl = "https://accounts.google.com/o/oauth2/auth";
            authUrl += $"?client_id={clientID}";
            authUrl += "&response_type=token";
            authUrl += $"&redirect_uri={WebUtility.UrlEncode(redirectUrl)}";
            authUrl += $"&scope={WebUtility.UrlEncode(string.Join(" ", scopes))}";
            var callbackUrl = await WebExtensions.Identity.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
            {
                Interactive = true,
                Url = new HttpURL(authUrl)
            });
            await InvokeAsync(async () =>
            {
                Token = callbackUrl;
                try
                {
                    var token = callbackUrl.Split("#")[1].Split("&")[0].Split("=")[1];
                    var driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = new TokenBaseInitializer(token),
                        GZipEnabled = false,
                    });
                    var ms = new MemoryStream();
                    await using var streamWriter = new StreamWriter(ms);
                    await streamWriter.WriteLineAsync("Yes");
                    await streamWriter.FlushAsync();
                    ms.Seek(0, SeekOrigin.Begin);

                    var file = new File()
                    {
                        Name = "config.json",
                        Parents = new List<string>
                        {
                            "appDataFolder"
                        },
                    };
                    var updateRequest = driveService.Files.Create(file, ms, "application/json");
                    await updateRequest.UploadAsync();
                    Console.WriteLine(updateRequest.ResponseBody.Id);
                    var listRequest = driveService.Files.List();
                    listRequest.Spaces = "appDataFolder";
                    listRequest.Fields = "nextPageToken, files(id, name)";
                    var fileList = await listRequest.ExecuteAsync();
                    foreach (var fileListFile in fileList.Files)
                    {
                        Console.WriteLine(fileListFile.Name);
                    }

                    var getRequest = driveService.Files.Get("1hSG8o9Fy7eagSiiDyQxoV3nchl2Q16ME_EDbGed1q3xZ9r81SQ");
                    await using var memoryStream = new MemoryStream();
                    var configFile = await getRequest.DownloadAsync(memoryStream);
                    var data = Encoding.UTF8.GetString(memoryStream.ToArray());
                    Console.WriteLine(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }

    public class TokenBaseInitializer : IConfigurableHttpClientInitializer
    {
        private readonly string _token;

        public TokenBaseInitializer(string token)
        {
            _token = token;
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.MessageHandler.Credential = new TokenCredential(_token);
        }
    }

    public class TokenCredential : IHttpExecuteInterceptor
    {
        private readonly string _token;

        public TokenCredential(string token)
        {
            _token = token;
        }

        public Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            // request.Headers.AcceptEncoding.Clear();
            // request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            return Task.CompletedTask;
        }
    }
}