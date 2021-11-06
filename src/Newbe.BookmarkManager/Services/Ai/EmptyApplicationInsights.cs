using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorApplicationInsights;
using Microsoft.JSInterop;

namespace Newbe.BookmarkManager.Services.Ai
{
    public class EmptyApplicationInsights : IApplicationInsights
    {
        public Task InitBlazorApplicationInsightsAsync(IJSRuntime jSRuntime)
        {
            return Task.CompletedTask;
        }

        public Task TrackEvent(string name, Dictionary<string, object>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task TrackTrace(string message, SeverityLevel? severityLevel = null,
            Dictionary<string, object>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task TrackException(Error exception, string? id = null, SeverityLevel? severityLevel = null,
            Dictionary<string, object>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task TrackPageView(string? name = null, string? uri = null, string? refUri = null,
            string? pageType = null,
            bool? isLoggedIn = null, Dictionary<string, object>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task StartTrackPage(string? name = null)
        {
            return Task.CompletedTask;
        }

        public Task StopTrackPage(string? name = null, string? url = null,
            Dictionary<string, string>? properties = null,
            Dictionary<string, decimal>? measurements = null)
        {
            return Task.CompletedTask;
        }

        public Task TrackMetric(string name, double average, double? sampleCount = null, double? min = null,
            double? max = null,
            Dictionary<string, object>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task TrackDependencyData(string id, string name, decimal? duration = null, bool? success = null,
            DateTime? startTime = null, int? responseCode = null, string? correlationContext = null,
            string? type = null,
            string? data = null, string? target = null)
        {
            return Task.CompletedTask;
        }

        public Task Flush(bool? async)
        {
            return Task.CompletedTask;
        }

        public Task ClearAuthenticatedUserContext()
        {
            return Task.CompletedTask;
        }

        public Task SetAuthenticatedUserContext(string authenticatedUserId, string? accountId = null,
            bool storeInCookie = false)
        {
            return Task.CompletedTask;
        }

        public Task AddTelemetryInitializer(TelemetryItem telemetryItem)
        {
            return Task.CompletedTask;
        }

        public Task TrackPageViewPerformance(PageViewPerformanceTelemetry pageViewPerformance)
        {
            return Task.CompletedTask;
        }

        public Task StartTrackEvent(string name)
        {
            return Task.CompletedTask;
        }

        public Task StopTrackEvent(string name, Dictionary<string, string>? properties = null,
            Dictionary<string, decimal>? measurements = null)
        {
            return Task.CompletedTask;
        }

        public Task SetInstrumentationKey(string key)
        {
            return Task.CompletedTask;
        }

        public Task LoadAppInsights()
        {
            return Task.CompletedTask;
        }

        public bool EnableAutoRouteTracking { get; set; } = false;
    }
}