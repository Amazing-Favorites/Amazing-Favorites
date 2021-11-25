using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.Servers;
using Newbe.BookmarkManager.Services.SimpleData;

namespace Newbe.BookmarkManager.Components
{
    public partial class NotificationCenter
    {
        [Inject] public INotificationCenterCore NotificationCenterCore { get; set; }

        protected override Task OnInitializedAsync()
        {
            NotificationCenterCore.StateChangeHandler = () => InvokeAsync(StateHasChanged);
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await NotificationCenterCore.InitAsync();
                StateHasChanged();
            }
        }

        private async Task ClickNotificationCenterAsync()
        {
            await NotificationCenterCore.MarkAsReadAsync();
        }
    }

    public interface IComponentService
    {
        Func<Task> StateChangeHandler { get; set; }
    }

    public interface INotificationCenterCore : IComponentService
    {
        bool RedDot { get; }
        bool NewMessage { get; }
        List<NotificationRecord> Records { get; }
        Task InitAsync();
        Task MarkAsReadAsync();
    }

    public class NotificationCenterCore : INotificationCenterCore
    {
        private readonly INotificationRecordService _notificationRecordService;
        private readonly IAfEventHub _afEventHub;
        
        private readonly ILPCClient<INotificationRecordServer> _lpcClient;

        public NotificationCenterCore(
            INotificationRecordService notificationRecordService,
            IAfEventHub afEventHub,
            ILPCClient<INotificationRecordServer> lpcClient)
        {
            _notificationRecordService = notificationRecordService;
            _afEventHub = afEventHub;
            _lpcClient = lpcClient;
        }

        public bool RedDot { get; private set; }
        public bool NewMessage { get; private set; }
        public List<NotificationRecord> Records { get; private set; } = new();

        public async Task InitAsync()
        {
            _afEventHub.RegisterHandler<NewNotificationEvent>(HandleNewNotificationEvent);
            await _lpcClient.StartAsync();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Records = await _notificationRecordService.GetListAsync();
            // var status = await _notificationRecordService.GetNewMessageStatusAsync();
            Records = (await _lpcClient
                .InvokeAsync<GetListNotificationRecordRequest, NotificationRecordResponse<List<NotificationRecord>>>( new GetListNotificationRecordRequest())).Data;
            
            Console.WriteLine("Records: " + Records.Count);

            foreach (var item in Records)
            {
                Console.WriteLine("item: " + item.Id);
            }
            
            var status = (await _lpcClient
                .InvokeAsync<GetNewMessageStatusNotificationRequest, NotificationRecordResponse<bool>>(
                    new GetNewMessageStatusNotificationRequest())).Data;
            RedDot = status;
        }

        private async Task HandleNewNotificationEvent(NewNotificationEvent arg)
        {
            Console.WriteLine("NewNotificationEvent");
            await LoadDataAsync();
            NewMessage = true;
            await StateChangeHandler.Invoke();
        }

        public async Task MarkAsReadAsync()
        {
            await _notificationRecordService.MakeAsReadAsync();
            RedDot = false;
            NewMessage = false;
        }

        public Func<Task> StateChangeHandler { get; set; }
    }
}