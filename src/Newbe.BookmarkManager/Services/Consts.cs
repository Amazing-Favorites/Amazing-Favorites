using System;
using System.Collections.Generic;

namespace Newbe.BookmarkManager.Services
{
    public static class Consts
    {
        public const string AppName = "Amazing Favorites";
        public const string CurrentVersion = "0.6.3";
        public const string PrivacyAgreementVersionDate = "2021/08/29";

        public static readonly string[] Versions =
        {
            CurrentVersion,
            "0.6.2",
            "0.6.1"
        };

        public const int JwtExpiredWarningDays = 7;

        public static class Commands
        {
            public const string OpenManager = "open-manager-ui";
        }

        public static class Cloud
        {
            public const string CloudDataFileName = "af.data.json";
        }

        public const string ManagerTabTitle = AppName;
        public const string AmazingFavoriteFolderName = AppName;

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