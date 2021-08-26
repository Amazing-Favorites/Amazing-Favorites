using System;
using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public static class Consts
    {
        public const string CurrentVersion = "0.6.1";
        public const string PrivacyAgreementVersionDate = "2021/08/17";

        public static readonly string[] Versions =
        {
            CurrentVersion
        };

        public const int JwtExpiredWarningDays = 7;

        public static class Commands
        {
            public const string OpenManager = "open-manager-ui";
        }

        public const string ManagerTabTitle = "Amazing Favorites";
        public const string AmazingFavoriteFolderName = "Amazing Favorites";

        public const string AfCodeSchemaPrefix = "af://";
        public const string DbName = "Amazing Favorites";

        public const string SingleOneDataId = "Amazing Favorites";

        public static class StoreNames
        {
            public const string Bks = "Bks";
            public const string BkMetadata = "BkMetadata";
            public const string Tags = "Tags";
            public const string UserOptions = "UserOptions";
            public const string AfMetadata = "AfMetadata";
            public const string SearchRecord = "SearchRecord";
            [Obsolete("use simple data")] public const string RecentSearch = "RecentSearch";
            public const string SimpleData = "SimpleData";
            public const string NotificationRecord = "NotificationRecord";
        }

        private static readonly HashSet<string> ReservedBookmarkFolder = new()
        {
            AmazingFavoriteFolderName,
            "Favorites bar",
            "Other favorites"
        };

        public static bool IsReservedBookmarkFolder(string title)
        {
            return ReservedBookmarkFolder.Contains(title);
        }
    }
}