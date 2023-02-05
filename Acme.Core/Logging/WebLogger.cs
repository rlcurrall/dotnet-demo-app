using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging
{
    public class WebLogger<TCategoryName> : ILogger<TCategoryName>
    {
        private readonly ILogger _logger;

        public WebLogger(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger =
                loggerFactory.CreateLogger(
                    TypeNameHelper.GetTypeDisplayName(typeof(TCategoryName), true, false, false, '.'));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
            => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state)
            => _logger.BeginScope(state);
    }
}