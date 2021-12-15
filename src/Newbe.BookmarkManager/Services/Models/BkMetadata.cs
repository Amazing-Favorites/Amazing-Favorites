using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services;

[Table(Consts.StoreNames.BkMetadata)]
public record BkMetadata : IEntity<string>
{
    public string Id { get; set; } = Consts.SingleOneDataId;
    public long LastUpdateTime { get; set; }
    public long EtagVersion { get; set; }
}