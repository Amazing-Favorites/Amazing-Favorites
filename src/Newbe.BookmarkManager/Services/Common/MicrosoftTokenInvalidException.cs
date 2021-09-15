using System;

namespace Newbe.BookmarkManager.Services
{
    public class MicrosoftTokenInvalidException : Exception
    {

        public MicrosoftTokenInvalidException(string message) : base(message)
        {

        }

    }
}