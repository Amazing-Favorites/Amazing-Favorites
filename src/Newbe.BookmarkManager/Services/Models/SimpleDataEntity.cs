using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.SimpleData)]
    public record SimpleDataEntity : IEntity<string>
    {
        public string Id { get; set; }
        public string PayloadJson { get; set; }
    }
}