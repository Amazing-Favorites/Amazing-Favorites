using System;
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
        public ApplicationInsightFeature? ApplicationInsightFeature { get; set; }
    }

    public record PinyinFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }

        public DateTime? Expire { get; set; }
    }

    public record CloudBkFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }

        public DateTime? Expire { get; set; }
    }

    public record HotTagsFeature
    {
        public bool Enabled { get; set; }
        public int ListCount { get; set; }
    }

    public record ApplicationInsightFeature
    {
        public bool Enabled { get; set; }
    }
}