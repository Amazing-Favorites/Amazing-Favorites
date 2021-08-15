namespace Newbe.BookmarkManager.Services.Configuration
{
    public record StaticUrlOptions
    {
        public string Docs { get; set; }
        public string WhatsNew { get; set; }
        public string PrivacyAgreement { get; set; }
        public string Welcome { get; set; }
    }
}