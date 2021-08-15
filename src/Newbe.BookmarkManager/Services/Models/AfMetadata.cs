using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.AfMetadata)]
    public record AfMetadata : IEntity<string>
    {
        public string Id { get; set; } = Consts.SingleOneDataId;
        public string WhatsNewVersion { get; set; }
        public bool WelcomeShown { get; set; }
    }
}