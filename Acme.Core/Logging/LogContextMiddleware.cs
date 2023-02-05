using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging
{
    public class LogContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogContextMiddleware> _logger;
        private readonly WebLoggerOptions _webLoggerOptions;

        /// <summary>
        /// Log Context middleware adds the main trace telemetry properties (e.g. App ID, EventId, and Correlation ID) to the logging scope
        /// Correlation ID will be added to the request header if it does not exist.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public LogContextMiddleware(RequestDelegate next, ILogger<LogContextMiddleware> logger,
            WebLoggerOptions options)
        {
            _next = next;
            _logger = logger;
            _webLoggerOptions = options;
        }

        public Task Invoke(HttpContext context)
        {
            Dictionary<string, object> requestTelemetryData =
                new Dictionary<string, object>
                {
                    {TraceTelemetryProperties.AppId, _webLoggerOptions.AppId},
                    {TraceTelemetryProperties.EventId, Guid.NewGuid()}
                };

            context.Request.Headers.TryGetValue(_webLoggerOptions.CorrelationIdHeaderKey,
                out var correlationIds);

            if (!correlationIds.Any())
            {
                context.Request.Headers.TryAdd(_webLoggerOptions.CorrelationIdHeaderKey, Guid.NewGuid().ToString());
            }

            if (context.Request.Headers.ContainsKey(_webLoggerOptions.CorrelationIdHeaderKey))
            {
                requestTelemetryData.Add(TraceTelemetryProperties.CorrelationId,
                    context.Request.Headers[_webLoggerOptions.CorrelationIdHeaderKey]);
            }

            using (_logger.BeginScope(requestTelemetryData))
            {
                return _next(context);
            }
        }
    }
}