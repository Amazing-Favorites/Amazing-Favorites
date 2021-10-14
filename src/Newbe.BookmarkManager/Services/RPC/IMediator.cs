using System;
using System.Threading.Tasks;
using Autofac;
using WebExtensions.Net.Runtime;

namespace Newbe.BookmarkManager.Services.RPC
{
    public interface IMediator : ISender
    {
        Task EnsureStartAsync();
        void RegisterHandler<TRequest>(Func<ILifetimeScope, IRequest, object> func);
    }
    public static class AfMediatorHubExtensions
    {
        public static void RegisterHandler<TRequest>(this IMediator afMediator, Func<TRequest, Task> handler)
            where TRequest : class, IRequest
        {
            afMediator.RegisterHandler<TRequest>((scope, re) => handler.Invoke((TRequest)re));
        }
    }
}