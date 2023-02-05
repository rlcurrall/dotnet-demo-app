using System.Runtime.CompilerServices;
using Acme.Core.Logging.StringHandlers;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging
{
    public static class WebLoggerExtensions
    {
        public static void LogTrace<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingTraceInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Trace, template, arguments);
        }

        public static void LogDebug<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingDebugInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Debug, template, arguments);
        }

        public static void LogInformation<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingInformationInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Information, template, arguments);
        }

        public static void LogWarning<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingWarningInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Warning, template, arguments);
        }

        public static void LogError<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingErrorInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Error, template, arguments);
        }

        public static void LogCritical<T>(
            this ILogger<T> logger,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingCriticalInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Critical, template, arguments);
        }

        public static void LogError<T>(
            this ILogger<T> logger,
            Exception exception,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingErrorInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Error, exception, template, arguments);
        }

        public static void LogCritical<T>(
            this ILogger<T> logger,
            Exception exception,
            [InterpolatedStringHandlerArgument("logger")]
            ref StructuredLoggingCriticalInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(LogLevel.Critical, exception, template, arguments);
        }

        public static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            [InterpolatedStringHandlerArgument("logger", "logLevel")]
            ref StructuredLoggingInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(logLevel, template, arguments);
        }

        public static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            Exception exception,
            [InterpolatedStringHandlerArgument("logger", "logLevel")]
            ref StructuredLoggingInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(logLevel, exception, template, arguments);
        }

        public static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            EventId eventId,
            [InterpolatedStringHandlerArgument("logger", "logLevel")]
            ref StructuredLoggingInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (!logger.IsEnabled(logLevel))
                return;
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(logLevel, eventId, (Exception) null, template, arguments);
        }

        public static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            EventId eventId,
            Exception exception,
            [InterpolatedStringHandlerArgument("logger", "logLevel")]
            ref StructuredLoggingInterpolatedStringHandler handler)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            if (!logger.IsEnabled(logLevel))
                return;
            var (template, arguments) = handler.GetTemplateAndArguments();
            logger.Log(logLevel, eventId, exception, template, arguments);
        }

        private static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            string message,
            object[] arg)
        {
            if (!logger.IsEnabled(logLevel))
                return;
            logger.Log(logLevel, (EventId) 0, (Exception) null, message, arg);
        }

        private static void Log<T>(
            this ILogger<T> logger,
            LogLevel logLevel,
            Exception exception,
            string message,
            object[] arguments)
        {
            if (!logger.IsEnabled(logLevel))
                return;
            logger.Log(logLevel, (EventId) 0, exception, message, arguments);
        }

        public static IDisposable BeginScope(
            this ILogger logger,
            string key,
            object value)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            return logger.BeginScope(new Dictionary<string, object> {{key, value}});
        }
    }
}