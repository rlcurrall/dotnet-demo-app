using System.Runtime.CompilerServices;
using Acme.Core.Logging.LogProviders;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging;

/// <summary>
///     Log class incorporates all the methods required for writing to console and Application Insights logs
/// </summary>
/// <example>
///     public int Main(string[] args) {
///     using (Log.Instance) {
///     Log.Instance.AddConsole("MYAPP");
///     // read configuration files, minimal setup etc here
///     Log.Instance.AddApplicationInsights("MYAPP", appInsightsConnection, defaultLogParameters);
///     // write the rest of the program, w/ log statements like:
///     Log.Instance.LogInfo("there was some information here", state: overrideLogParameters);
///     }
///     }
/// </example>
public sealed class Log : IDisposable, ILogger
{
    private readonly LogParameter _mDefaultParameters = new();

    private readonly List<BaseLogProvider> _mLoggers = new();

    private string _mApplicationCode = "UNKNOWN";

    private bool _mDisposedValue;

    /// <summary>
    ///     The default <see cref="Log" /> instance.
    /// </summary>
    public static Log Instance { get; } = new();

    /// <summary>
    ///     Log messages must be at least as severe as the <see cref="MinimumLogLevel" /> to be written to the log file.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    ///     The Application Id that will be logged with every log message. Should be an "Application Code" from ServiceNow
    ///     Applications list.
    ///     When set to a null, blank, or whitespace value, the property will not throw an exception, but it will ignore the
    ///     value and keep whatever it was using before.
    ///     Default value is "UNKNOWN"
    /// </summary>
    public string ApplicationCode
    {
        get => _mApplicationCode;

        set => _mApplicationCode = string.IsNullOrWhiteSpace(value) ? _mApplicationCode : value;
    }

    /// <summary>
    ///     Writes any remaining log entries to the log providers and waits for them to be written
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Returns true if a log message will be written at the given <paramref name="logLevel" />
    /// </summary>
    /// <param name="logLevel">The <see cref="LogLevel" /> to check against the <see cref="MinimumLogLevel" /> </param>
    /// <returns>true if a log message will be written at the given <paramref name="logLevel" />, false otherwise</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return MinimumLogLevel != LogLevel.None && logLevel >= MinimumLogLevel;
    }

    /// <summary>
    ///     Writes a log entry.
    /// </summary>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">
    ///     Function to create a <see cref="string" /> message of the <paramref name="state" /> and
    ///     <paramref name="exception" />.
    /// </param>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        // eventId is ignored
        Write(logLevel, formatter(state, exception), exception);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IDisposable BeginScope<TState>(TState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        return new DisposableScope();
    }

    /// <summary>
    ///     Registers the <paramref name="logger" /> to receive all log messages
    /// </summary>
    /// <param name="applicationCode">The Application Identifier for the application that the logger is logging for</param>
    /// <param name="logger">The <see cref="BaseLogProvider" /> to register as a log sink</param>
    /// <param name="defaults">
    ///     A <see cref="LogParameter" /> that provides default values to use in subsequent logging method
    ///     calls
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="applicationCode" /> is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger" /> is null</exception>
    public void AddLogger(string applicationCode, BaseLogProvider logger, LogParameter defaults = null)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        SetDefaultLogParameters(applicationCode, defaults);

        _mLoggers.Add(logger);
    }

    /// <summary>
    ///     Adds console logging to the log provider
    /// </summary>
    /// <param name="applicationCode">The Application Identifier for the application that the logger is logging for</param>
    /// <param name="defaults">
    ///     A <see cref="LogParameter" /> that provides default values to use in subsequent logging method
    ///     calls
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="applicationCode" /> is null or empty</exception>
    public void AddConsoleLogging(string applicationCode, LogParameter defaults = null)
    {
        AddLogger(applicationCode, new ConsoleLogProvider(), defaults);
    }

    /// <summary>
    ///     Adds Azure Application Inisghts logging to the log provider
    /// </summary>
    /// <param name="applicationCode">The Application Identifier for the application that the logger is logging for</param>
    /// <param name="appInsightsConnection">
    ///     The Application Insights Connection String or Instrumentation Key which identifies
    ///     the Azure App Insights instance to log to
    /// </param>
    /// <param name="defaults">
    ///     A <see cref="LogParameter" /> that provides default values to use in subsequent logging method
    ///     calls
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when either <paramref name="appInsightsConnection" /> or
    ///     <paramref name="applicationCode" /> is null or whitespace
    /// </exception>
    public void AddApplicationInsights(string applicationCode, string appInsightsConnection,
        LogParameter defaults = null)
    {
        if (string.IsNullOrWhiteSpace(appInsightsConnection))
            throw new ArgumentOutOfRangeException(nameof(appInsightsConnection),
                $"{nameof(appInsightsConnection)} cannot be empty");

        AddLogger(applicationCode, new AppInsightsLogProvider(appInsightsConnection), defaults);
    }

    private void SetDefaultLogParameters(string applicationCode, LogParameter defaults = null)
    {
        defaults = defaults ??
                   _mDefaultParameters; // if no other defaults are passed in, we won't want to overwrite the other default parameters

        ApplicationCode =
            applicationCode; // the property will not accept a null, empty, or whitespace value; but no exception will be thrown

        _mDefaultParameters.Domain ??= defaults.Domain;
        _mDefaultParameters.Flow ??= defaults.Flow;
        _mDefaultParameters.Stage ??= defaults.Stage;
        _mDefaultParameters.Direction ??= defaults.Direction;
        _mDefaultParameters.EventType ??= defaults.EventType;
        _mDefaultParameters.Status ??= defaults.Status;
        _mDefaultParameters.Description ??= defaults.Description;
        _mDefaultParameters.StageStatus ??= defaults.StageStatus;
        _mDefaultParameters.Metric1 ??= defaults.Metric1;
        _mDefaultParameters.Metric2 ??= defaults.Metric2;
    }

    private LogParameter CreateLogProperties(LogParameter logParameter)
    {
        if (logParameter == null) return _mDefaultParameters.Copy();

        logParameter.Description ??= _mDefaultParameters.Description;
        logParameter.Direction ??= _mDefaultParameters.Direction;
        logParameter.Domain ??= _mDefaultParameters.Domain;
        logParameter.EventType ??= _mDefaultParameters.EventType;
        logParameter.Flow ??= _mDefaultParameters.Flow;
        logParameter.Metric1 ??= _mDefaultParameters.Metric1;
        logParameter.Metric2 ??= _mDefaultParameters.Metric2;
        logParameter.Stage ??= _mDefaultParameters.Stage;
        logParameter.StageStatus ??= _mDefaultParameters.StageStatus;
        logParameter.Status ??= _mDefaultParameters.Status;

        return logParameter;
    }

    /// <summary>
    ///     This method writes information about copy operations such as start time,end time
    /// </summary>
    /// <param name="message">Message containing information about the copy operation</param>
    [Obsolete("Use the Log.LogInfo() method instead")]
    public static void WriteInfo(string message)
    {
        Console.WriteLine(message);
    }


    /// <summary>
    ///     This method writes the error captured during copy operation to log properties
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageDetails"></param>
    [Obsolete("Use the Log.LogError() method instead")]
    public static void WriteError(string message, string messageDetails)
    {
        Console.WriteLine(message);
        Console.WriteLine(messageDetails);
    }


    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Trace" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogTrace(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Trace, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Debug" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogDebug(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Debug, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Information" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("Use the Log.LogInformation() method instead")]
    public void LogInfo(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Information, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Information" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogInformation(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Information, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Warning" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogWarning(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Warning, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Error" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogError(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Error, message, error, state, args);
    }

    /// <summary>
    ///     Writes a log message if the <see cref="LogLevel.Critical" /> level or higher is enabled.
    /// </summary>
    /// <param name="message">The message to write to the log</param>
    /// <param name="error">Optional. The exception that caused the trace</param>
    /// <param name="state">Any overrides for the default <see cref="LogParameter" /></param>
    /// <param name="args">format arguments for the <paramref name="message" /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogCritical(string message, Exception error = null, LogParameter state = null, params object[] args)
    {
        Write(LogLevel.Critical, message, error, state, args);
    }

    private void Write(LogLevel atLevel, string message, Exception error = null, LogParameter state = null,
        params object[] args)
    {
        if (!IsEnabled(atLevel))
            return;

        var
            code = _mApplicationCode; // making a local copy so that all providers get the same code for each log message
        message = message ?? string.Empty;
        message = args == null || args.Length < 1 ? message : string.Format(message, args);

        for (var i = 0; i < _mLoggers.Count; ++i)
            try
            {
                var logProperties =
                    CreateLogProperties(
                        state); // getting a new one every time to be sure that any modifications are not passed from provider to provider
                _mLoggers[i].LogAsync(atLevel, code, message, logProperties, error);
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
#pragma warning restore CA1031
                Console.WriteLine(
                    $"{DateTime.Now.ToString(BaseLogProvider.TimestampFormat)} ERROR writing log message to the {_mLoggers[i]?.GetType().FullName} logger");
                Console.WriteLine(ex.ToString());
            }
    }


    /// <summary>
    ///     Writes any remaining log entries to the log providers and waits for them to be written
    /// </summary>
    [Obsolete("Use the Dispose() method instead")]
    public void TelemetryFlush()
    {
        Dispose();
    }

    /// <summary>
    ///     Writes any remaining log entries to the log providers and waits for them to be written
    /// </summary>
    /// <param name="disposing">
    ///     <see cref="bool.TrueString" /> if called from the Dispose() method;
    ///     <see cref="bool.FalseString" /> if called from the Finalizer
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_mDisposedValue)
            return;

        if (disposing)
            foreach (var t in _mLoggers)
                try
                {
                    //TODO: call Dispose() in parallel for all providers & move the ProcessExit logic in the AppInsights provider to the log class
                    t.Dispose();
                }
#pragma warning disable CA1031
                catch (Exception ex)
                {
#pragma warning restore CA1031
                    Console.WriteLine(
                        $"{DateTime.Now.ToString(BaseLogProvider.TimestampFormat)} ERROR flushing telemetry for the {t?.GetType().FullName} logger");
                    Console.WriteLine(ex.ToString());
                }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        _mDisposedValue = true;
    }

    private class DisposableScope : IDisposable
    {
        public void Dispose()
        {
            // don't need to do anything
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Log()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
}