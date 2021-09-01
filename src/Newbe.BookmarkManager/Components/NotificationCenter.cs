using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.EventHubs;
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

        public NotificationCenterCore(
            INotificationRecordService notificationRecordService,
            IAfEventHub afEventHub)
        {
            _notificationRecordService = notificationRecordService;
            _afEventHub = afEventHub;
        }

        public bool RedDot { get; private set; }
        public bool NewMessage { get; private set; }
        public List<NotificationRecord> Records { get; private set; } = new();

        public async Task InitAsync()
        {
            _afEventHub.RegisterHandler<NewNotificationEvent>(HandleNewNotificationEvent);
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Records = await _notificationRecordService.GetListAsync();
            var status = await _notificationRecordService.GetNewMessageStatusAsync();
            RedDot = status;
        }

        private async Task HandleNewNotificationEvent(NewNotificationEvent arg)
        {
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