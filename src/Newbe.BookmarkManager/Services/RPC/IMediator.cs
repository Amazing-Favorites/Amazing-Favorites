using System;
using System.Threading.Tasks;
using WebExtensions.Net.Runtime;

namespace Newbe.BookmarkManager.Services.RPC
{
    public interface IMediator :ISender
    {
        public Task<bool> OnSendMessage(object message, MessageSender sender, Action<object> sendResponse);
        Task EnsureStartAsync();
    }
}