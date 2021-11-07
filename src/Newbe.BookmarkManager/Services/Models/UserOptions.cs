using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newbe.BookmarkManager.Services
{
    [Table(Consts.StoreNames.UserOptions)]
    public record UserOptions : IEntity<string>
    {
        public string Id => Consts.SingleOneDataId;

        public bool AcceptPrivacyAgreement => AcceptPrivacyAgreementVersion == Consts.PrivacyAgreementVersionDate;
        public bool AcceptPrivacyAgreementBefore { get; set; }
        public string AcceptPrivacyAgreementVersion { get; set; } = string.Empty;
        public PinyinFeature? PinyinFeature { get; set; }
        public CloudBkFeature? CloudBkFeature { get; set; }
        public HotTagsFeature? HotTagsFeature { get; set; }
        public ApplicationInsightFeature? ApplicationInsightFeature { get; set; }

        public OmniboxSuggestFeature? OmniboxSuggestFeature { get; set; }
    }

    public record PinyinFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }

        public DateTime? ExpireDate { get; set; }
    }

    public record CloudBkFeature
    {
        public bool Enabled { get; set; }
        public string? BaseUrl { get; set; }
        public string? AccessToken { get; set; }

        public DateTime? ExpireDate { get; set; }
        public CloudBkProviderType CloudBkProviderType { get; set; }
    }

    public enum CloudBkProviderType
    {
        NewbeApi,
        GoogleDrive,
        OneDrive
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

    public record OmniboxSuggestFeature
    {
        public bool Enabled { get; set; }
        private int _suggestCount;

        public int SuggestCount
        {
            get =>
                _suggestCount;
            set => _suggestCount = Math.Clamp(value, Consts.Omnibox.SuggestMin, Consts.Omnibox.SuggestMax);
        }
    }
}