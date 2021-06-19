using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.UserOptions)]
    public record UserOptions : IEntity<string>
    {
        public string Id => Consts.SingleOneDataId;
        public bool? AcceptPrivacyAgreement { get; set; }
        public PinyinFeature? PinyinFeature { get; set; }
        public CloudBkFeature? CloudBkFeature { get; set; }
        public HotTagsFeature? HotTagsFeature { get; set; }
    }

    public record PinyinFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }
    }

    public record CloudBkFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }
    }

    public record HotTagsFeature
    {
        public bool Enabled { get; set; }
        public int ListCount { get; set; }
    }
}