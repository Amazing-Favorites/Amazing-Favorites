using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background : IAsyncDisposable
    {
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] public ISyncBookmarkJob SyncBookmarkJob { get; set; } = null!;
        [Inject] public ISyncAliasJob SyncAliasJob { get; set; } = null!;
        [Inject] public ISyncCloudJob SyncCloudJob { get; set; } = null!;
        [Inject] public IDataFixJob DataFixJob { get; set; } = null!;
        [Inject] public ISyncTagRelatedBkCountJob SyncTagRelatedBkCountJob { get; set; } = null!;
        [Inject] public IShowWhatNewJob ShowWhatNewJob { get; set; } = null!;
        [Inject] public IShowWelcomeJob ShowWelcomeJob { get; set; } = null!;
        [Inject] public IUserOptionsService UserOptionsService { get; set; } = null!;
        [Inject] public IAfEventHub AfEventHub { get; set; }
        [Inject] public IGoogleDriveClient GoogleDriveClient { get; set; }

        private JsModuleLoader _moduleLoader;

        [JSInvokable]
        public void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                AfEventHub.RegisterHandler<UserGoogleDriveLoginSuccessEvent>(HandleUserGoogleLoginAsync);
                AfEventHub.RegisterHandler<GoogleTryLoginInBackgroundEvent>(HandleGoogleTryLoginInBackgroundEvent);
                await AfEventHub.StartAsync();
                _moduleLoader = new JsModuleLoader(JsRuntime);
                await _moduleLoader.LoadAsync("/content/background_keyboard.js");
                var userOptions = await UserOptionsService.GetOptionsAsync();
                if (userOptions is
                    {
                        AcceptPrivacyAgreement: true,
                        ApplicationInsightFeature:
                        {
                            Enabled: true
                        }
                    })
                {
                    await _moduleLoader.LoadAsync("/content/ai.js");
                }

                var lDotNetReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("DotNet.SetDotnetReference", lDotNetReference);
                await DataFixJob.StartAsync();
                await ShowWelcomeJob.StartAsync();
                await ShowWhatNewJob.StartAsync();
                await SyncBookmarkJob.StartAsync();
                await SyncAliasJob.StartAsync();
                await SyncCloudJob.StartAsync();
                await SyncTagRelatedBkCountJob.StartAsync();
            }
        }

        private async Task HandleGoogleTryLoginInBackgroundEvent(GoogleTryLoginInBackgroundEvent afEvent)
        {
            var loginResult = await GoogleDriveClient.LoginAsync(false);
            await AfEventHub.PublishAsync(new GoogleBackgroundLoginResultEvent
            {
                Success = loginResult
            });
        }

        private async Task HandleUserGoogleLoginAsync(UserGoogleDriveLoginSuccessEvent afEvent)
        {
            Logger.LogInformation("received {Event}", nameof(UserGoogleDriveLoginSuccessEvent));
            await GoogleDriveClient.LoginAsync(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _moduleLoader.DisposeAsync();
        }
    }
}