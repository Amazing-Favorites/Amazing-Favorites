namespace Newbe.BookmarkManager.Services
{
    /// <summary>
    /// Authentication options for the underlying msal.js library handling the authentication.
    /// </summary>
    public record OneDriveOAuthOptions
    {
        /// <summary>
        /// Gets or sets the client id for the application.
        /// </summary>
        public string? ClientId { get; set; }

        public OAuth2ClientType Type { get; set; }
        public string? DevClientId { get; set; }

        /// <summary>
        /// Gets or sets the authority for the Azure Active Directory or Azure Active Directory B2C instance.
        /// </summary>
        public string? Authority { get; set; }

        /// <summary>
        /// Gets or sets whether or not to navigate to the login request url after a successful login.
        /// </summary>
        public bool NavigateToLoginRequestUrl { get; set; }

        public string[] DefaultScopes { get; set; }
    }
}