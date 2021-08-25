using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newbe.BookmarkManager.WebApi;

namespace Newbe.BookmarkManager.Services
{
    public interface IOneDriveClient
    {
        Task<bool> LoginAsync(bool interactive);
        Task<User> GetMeAsync();
        Task<Drive> GetOneDriveAsync();
        Task<IEnumerable<DriveItem>> GetDriveContentsAsync();
        Task<Stream> GetFileStreamByItemId(string id);
        Task<IEnumerable<DriveItem>> SearchFileFromDriveAsync(string searchString);
        Task<DriveItem> UploadingFileAsync(Stream fileStream, string itemPath);

        Task<CloudDataDescription?> GetFileDescriptionAsync();
        Task<CloudBkCollection?> GetCloudDataAsync();
        Task UploadAsync(CloudBkCollection cloudBkCollection);
        Task<bool> TestAsync();
    }

    /// <summary>
    /// Authentication options for the underlying msal.js library handling the authentication.
    /// </summary>
    public record OneDriveOAuthOptions
    {
        /// <summary>
        /// Gets or sets the client id for the application.
        /// </summary>
        public string ClientId { get; set; }

        public OAuth2ClientType Type { get; set; }
        public string DevClientId { get; set; }

        /// <summary>
        /// Gets or sets the authority for the Azure Active Directory or Azure Active Directory B2C instance.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether or not to validate the authority.
        /// </summary>
        /// <remarks>
        /// This value needs to be set to false when using Azure Active Directory B2C.
        /// </remarks>
        public bool ValidateAuthority { get; set; } = true;

        /// <summary>
        /// Gets or sets the redirect uri for the application.
        /// </summary>
        /// <remarks>
        /// It can be an absolute or base relative <see cref="Uri"/> and defaults to <c>authentication/login-callback.</c>
        /// </remarks>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the post logout redirect uri for the application.
        /// </summary>
        /// <remarks>
        /// It can be an absolute or base relative <see cref="Uri"/> and defaults to <c>authentication/logout-callback.</c>
        /// </remarks>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets whether or not to navigate to the login request url after a successful login.
        /// </summary>
        public bool NavigateToLoginRequestUrl { get; set; }

        /// <summary>
        /// Gets or sets the set of known authority host names for the application.
        /// </summary>
        public IList<string> KnownAuthorities { get; set; } = new List<string>();

        public string[] DefaultScopes { get; set; }
    }
}