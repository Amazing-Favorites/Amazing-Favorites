using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebExtension.Net.Bookmarks;

namespace Newbe.BookmarkManager.Services
{
    public interface IBkDataHolder
    {
        BkEntityCollection Collection { get; }
        ValueTask AppendBookmarksAsync(IEnumerable<BookmarkTreeNode> nodes);
        ValueTask SaveNowAsync();
        ValueTask RestoreAsync();
        ValueTask InitAsync();
        Task PushDataChangeActionAsync(Func<Task> action);
        event EventHandler<EventArgs> OnDataReload;
    }
}