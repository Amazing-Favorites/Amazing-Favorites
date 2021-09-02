using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.NotificationRecord)]
    public record NotificationRecord : IEntity<long>
    {
        public long Id { get; set; }
        public List<MsgItem> Items { get; set; } = null!;
        public long UpdateTime { get; set; }
        public string Group { get; set; } = null!;
    }

    public record MsgItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public UserNotificationType Type { get; set; }
        public string ArgsJson { get; set; }
        public long CreatedTime { get; set; }
    }

    public enum UserNotificationType
    {
        Normal,
        Welcome,
        NewRelease,
        SyncDataWithCloud,
        PrivacyAgreementUpdated,
        PinyinTokenExpired,
        CloudBkTokenExpired,
        SuccessToSyncBkWithCloud
    }
}