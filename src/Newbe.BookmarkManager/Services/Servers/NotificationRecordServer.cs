﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services.Servers;

public class NotificationRecordServer : INotificationRecordServer
{

    private readonly INotificationRecordService _notificationRecordService;

    public NotificationRecordServer(INotificationRecordService notificationRecordService)
    {
        _notificationRecordService = notificationRecordService;
    }
    public async Task<NotificationRecordResponse> AddAsync(AddNotificationRecordRequest request)
    {
        await _notificationRecordService.AddAsync(request.Item);

        return new NotificationRecordResponse();
    }

    public async Task<NotificationRecordResponse<List<NotificationRecord>>> GetListAsync(GetListNotificationRecordRequest request)
    {
        var result = await _notificationRecordService.GetListAsync();
        return new NotificationRecordResponse<List<NotificationRecord>>()
        {
            Data = result
        };
    }

    public async Task<NotificationRecordResponse> MakeAsReadAsync(MakeAsReadNotificationRecordRequest request)
    {
        await _notificationRecordService.MakeAsReadAsync();

        return new NotificationRecordResponse();
    }

    public async Task<NotificationRecordResponse<bool>> GetNewMessageStatusAsync(GetNewMessageStatusNotificationRequest request)
    {
        var result = await _notificationRecordService.GetNewMessageStatusAsync();
        return new NotificationRecordResponse<bool>()
        {
            Data = result
        };
    }
}