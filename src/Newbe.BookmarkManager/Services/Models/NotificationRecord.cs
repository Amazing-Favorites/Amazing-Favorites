using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newbe.BookmarkManager.Services.EventHubs;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.NotificationRecord)]
    public record NotificationRecord : IEntity<long>
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public AfNotificationType AfNotificationType { get; set; }

        public bool Read { get; set; }
        public DateTime CreatedTime { get; set; }  
        public DateTime? CompletionTime { get; set; }
    }

}