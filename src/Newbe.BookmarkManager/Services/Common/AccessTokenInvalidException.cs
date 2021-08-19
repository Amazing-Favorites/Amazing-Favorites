using System;

namespace Newbe.BookmarkManager.Services.Common
{
    public class AccessTokenInvalidException : Exception
    {
        public string AccessToken { get; set; }

        public AccessTokenInvalidException(string accessToken) :
            this($"The access token {accessToken} is invalid", accessToken)
        {
        }

        public AccessTokenInvalidException(string message, string accessToken) : base(message)
        {
            AccessToken = accessToken;
        }
    }
}