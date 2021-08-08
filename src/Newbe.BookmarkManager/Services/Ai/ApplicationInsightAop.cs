using System;
using System.Reflection;
using System.Threading.Tasks;
using BlazorApplicationInsights;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services.Ai
{
    public class ApplicationInsightAop : AsyncInterceptorBase, IInterceptor
    {
        public static bool Enabled { get; set; } = false;
        private readonly ILogger<ApplicationInsightAop> _logger;
        private readonly IApplicationInsights _insights;

        public ApplicationInsightAop(
            ILogger<ApplicationInsightAop> logger,
            IApplicationInsights insights)
        {
            _logger = logger;
            _insights = insights;
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
            Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            if (Enabled)
            {
                var loggingAttribute = invocation.Method.GetCustomAttribute<InsightAttribute>();
                var eventName = loggingAttribute?.EventName;
                if (eventName != null)
                {
                    await _insights.StartTrackEvent(eventName);
                }

                try
                {
                    await proceed(invocation, proceedInfo).ConfigureAwait(false);
                    return;
                }
                finally
                {
                    if (eventName != null)
                    {
                        await _insights.StopTrackEvent(eventName);
                    }
                }
            }

            await proceed(invocation, proceedInfo).ConfigureAwait(false);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
            IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            if (Enabled)
            {
                var loggingAttribute = invocation.Method.GetCustomAttribute<InsightAttribute>();
                var eventName = loggingAttribute?.EventName;
                if (eventName != null)
                {
                    await _insights.StartTrackEvent(eventName);
                }

                try
                {
                    return await proceed(invocation, proceedInfo).ConfigureAwait(false);
                }
                finally
                {
                    if (eventName != null)
                    {
                        await _insights.StopTrackEvent(eventName);
                    }
                }
            }

            return await proceed(invocation, proceedInfo).ConfigureAwait(false);
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }
    }
}