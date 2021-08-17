using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.SearchRecord)]
    public record SearchRecord : IEntity<long>
    {
        public long Id { get; set; }
        public string RecordJson { get; set; }
    }
}