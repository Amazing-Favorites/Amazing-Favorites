using System;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.EventHubs
{
    public class FuncEventHandler : IAfEventHandler
    {
        private readonly Func<IAfEvent, Task> _func;

        public FuncEventHandler(
            Func<IAfEvent, Task> func)
        {
            _func = func;
        }

        public Task HandleAsync(IAfEvent afEvent)
        {
            return _func.Invoke(afEvent);
        }
    }
}