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
        Task PushDataChangeActionAsync(Func<Task<bool>> action);
        event EventHandler<EventArgs> OnDataReload;
    }

    public static class BkDataHolderExtensions
    {
        public static Task PushDataChangeActionAsync(this IBkDataHolder holder, Action action)
        {
            return holder.PushDataChangeActionAsync(() =>
            {
                action.Invoke();
                return Task.FromResult(true);
            });
        }
    }
}