using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.RPC;

namespace Newbe.BookmarkManager.Pages
{
    public partial class Background
    {
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] public IUserOptionsService UserOptionsService { get; set; } = null!;
        [Inject] public IJobHost JobHost { get; set; }
        [Inject] public IMediator Mediator { get; set; }

        [Inject] public IIndexedDbRepo<Bk, string> _bkRepo { get; set; }

        private UserOptions _userOptions = null!;

        private void OnReceivedCommand(string command)
        {
            if (command == Consts.Commands.OpenManager)
            {
#pragma warning disable 4014
                WebExtensions.Tabs.ActiveOrOpenManagerAsync();
#pragma warning restore 4014
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _userOptions = await UserOptionsService.GetOptionsAsync();
            await WebExtensions.Commands.OnCommand.AddListener(OnReceivedCommand);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await JobHost.StartAsync();
                await Mediator.EnsureStartAsync();
                Mediator.RegisterHandler<GetAllBkRequest>(SearchHandler);
            }
        }


        public Task<List<Bk>> SearchHandler(GetAllBkRequest request)
        {
            var result = _bkRepo.GetAllAsync();

            return result;
        }
    }
}