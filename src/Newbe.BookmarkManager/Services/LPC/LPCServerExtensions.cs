using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newbe.BookmarkManager.Services.MessageBus;

namespace Newbe.BookmarkManager.Services.LPC
{
    public static class LPCServerExtensions
    {
        public static IReadOnlyList<MethodInfo> AddServerInstance<T>(this ILPCServer server, T instance)
        {
            var remoteMethods = typeof(T).GetMethods()
                .Where(IsRemoteMethod)
                .ToList();
            foreach (var method in remoteMethods)
            {
                AddHandlerCore1(server, instance!,
                    method,
                    method.GetParameters()[0].ParameterType,
                    method.ReturnType.GenericTypeArguments[0]);
            }

            return remoteMethods;
        }

        internal static bool IsRemoteMethod(MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length != 1)
            {
                return false;
            }

            if (parameterInfos[0].ParameterType.GetInterface(nameof(IRequest)) == null)
            {
                return false;
            }

            if (methodInfo.ReturnType.Name != typeof(Task<>).Name)
            {
                return false;
            }

            var returnType = methodInfo.ReturnType.GenericTypeArguments[0];
            if (returnType.GetInterface(nameof(IResponse)) == null)
            {
                return false;
            }

            return true;
        }

        private static void AddHandlerCore1(ILPCServer server,
            object instance,
            MethodInfo method,
            Type requestType,
            Type responseType)
        {
            var addHandlerCoreMethod = typeof(LPCServerExtensions).GetMethod(nameof(AddHandlerCore2),
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!;
            var makeGenericMethod = addHandlerCoreMethod.MakeGenericMethod(requestType, responseType);
            makeGenericMethod.Invoke(null, new[] { server, instance, method });
        }

        internal static void AddHandlerCore2<TRequest, TResponse>(ILPCServer server,
            object instance,
            MethodInfo method)
            where TRequest : IRequest
            where TResponse : IResponse
        {
            server.AddHandler<TRequest, TResponse>(req =>
            {
                var task = method.Invoke(instance, new object?[] { req })!;
                return (Task<TResponse>)task;
            });
        }
    }
}