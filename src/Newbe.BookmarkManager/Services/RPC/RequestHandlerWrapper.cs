using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Newbe.BookmarkManager.Services.RPC
{
 
    
    
    // public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    //     where TRequest : IRequest<TResponse>
    // {
    //     public override async Task<object?> Handle(object request) =>
    //         await Handle((IRequest<TResponse>)request).ConfigureAwait(false);
    //
    //     public override Task<TResponse> Handle(IRequest<TResponse> request)
    //     {
    //         Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);
    //
    //         return serviceFactory
    //             .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
    //             .Reverse()
    //             .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, next))();
    //     }
    // }
    
}