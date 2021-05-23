namespace Newbe.BookmarkManager.Services
{
    public static class Consts
    {
        public static class Commands
        {
            public const string OpenManager = "open-manager-ui";
        }

        public static class StorageKeys
        {
            public const string BookmarksData = "Newbe.BookmarkManager.Data.V1";
            public const string BookmarksDataLastUpdatedTime = "Newbe.BookmarkManager.Data.V1.LastUpdatedTime";
            public const string UserOptionsName = "Newbe.BookmarkManager.UserOptions.V1";
        }

        public const string ManagerTabTitle = "Amazing Favorites";
        public const string AmazingFavoriteFolderName = "Amazing Favorites";
    }
}