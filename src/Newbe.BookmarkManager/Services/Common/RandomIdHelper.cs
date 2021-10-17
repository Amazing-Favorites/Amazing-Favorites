namespace Newbe.BookmarkManager.Services
{
    public static class RandomIdHelper
    {
        public static string GetId() => Nanoid.Nanoid.Generate("1234567890abcdef", 10);
    }
}