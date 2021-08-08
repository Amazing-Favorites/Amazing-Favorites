using BlazorApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Newbe.BookmarkManager.Services.Ai
{
    public sealed class AiLoggerProvider : ILoggerProvider
    {
        private readonly IApplicationInsights _applicationInsights;
        private ILogger? _logger;
        private bool _disposed = false;

        public AiLoggerProvider(IApplicationInsights applicationInsights)
        {
            _applicationInsights = applicationInsights;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger ??= new ApplicationInsightsLogger(_applicationInsights);
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}