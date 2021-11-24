using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.Servers
{
    public interface INotificationRecordServer
    {
        Task<NotificationRecordResponse> AddAsync(AddNotificationRecordRequest request);
        Task<NotificationRecordResponse<List<NotificationRecord>>> GetListAsync(GetListNotificationRecordRequest request);
        Task<NotificationRecordResponse> MakeAsReadAsync(MakeAsReadNotificationRecordRequest request);
        Task<NotificationRecordResponse<bool>> GetNewMessageStatusAsync(GetNewMessageStatusNotificationRequest request);
    }
    
    public record GetListNotificationRecordRequest : IRequest
    {}

    public record MakeAsReadNotificationRecordRequest : IRequest
    {
        
    }
    
    public record GetNewMessageStatusNotificationRequest : IRequest
    {}
    public record AddNotificationRecordRequest : IRequest
    {
        public MsgItem Item { get; set; }
    }
    public record NotificationRecordResponse : IResponse
    {
    }
    public record NotificationRecordResponse<T> : NotificationRecordResponse
    {
        public T Data { get; init; }
    }
}