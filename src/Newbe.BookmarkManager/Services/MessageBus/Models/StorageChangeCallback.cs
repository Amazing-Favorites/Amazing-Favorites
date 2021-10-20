using System.Text.Json;

namespace Newbe.BookmarkManager.Services.MessageBus
{
    public delegate void StorageChangeCallback(JsonElement changes, string area);
}