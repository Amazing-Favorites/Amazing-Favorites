﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Newbe.BookmarkManager.Services.RPC
{
    // public abstract class RequestHandlerBase
    // {
    //     public abstract Task<object?> Handle(object request, CancellationToken cancellationToken,
    //         //ServiceFactory serviceFactory
    //         );
    //
    //     protected static THandler GetHandler<THandler>(
    //         //ServiceFactory factory
    //         )
    //     {
    //         THandler handler;
    //
    //         try
    //         {
    //             //handler = factory.GetInstance<THandler>();
    //         }
    //         catch (Exception e)
    //         {
    //             throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.", e);
    //         }
    //
    //         if (handler == null)
    //         {
    //             throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container. See the samples in GitHub for examples.");
    //         }
    //
    //         return handler;
    //     }
    // }
    // public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    // {
    //     public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
    //         //ServiceFactory serviceFactory
    //         );
    // }
    //
    // public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    //     where TRequest : IRequest<TResponse>
    // {
    //     public override async Task<object?> Handle(object request, CancellationToken cancellationToken,
    //         //ServiceFactory serviceFactory
    //         ) =>
    //         await Handle((IRequest<TResponse>)request, cancellationToken).ConfigureAwait(false);
    //
    //     public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
    //         //ServiceFactory serviceFactory
    //         )
    //     {
    //         Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>().Handle((TRequest) request, cancellationToken);
    //
    //         return serviceFactory
    //             .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
    //             .Reverse()
    //             .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
    //     }
    // }
    
}