using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.LogProviders;

/// <summary>
///     The base class for the log providers that the <see cref="Log" /> class uses to log messages
/// </summary>
public abstract class BaseLogProvider : IDisposable
{
    /// <summary>
    ///     The <see cref="DateTime" /> format string to use to write dates+times to log files
    /// </summary>
    public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    ///     <see cref="bool.TrueString" /> if the object has already been disposed; <see cref="bool.FalseString" /> if this
    ///     instance has not been disposed yet
    /// </summary>
#pragma warning disable CA1051
    protected bool MDisposedValue;
#pragma warning restore CA1051

    /// <summary>
    ///     Writes any remaining log entries to the log providers and waits for them to be written.
    ///     Frees all managed and unmanaged resources (memory, handles, etc)
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Logs Events Asynchronously
    /// </summary>
    /// <param name="appCode">The identifier for the application or integration</param>
    /// <param name="eventName">Event Name</param>
    /// <param name="properties">List of parameters to be logged</param>
    /// <param name="metrics">List of metrics to be logged</param>
    /// <returns></returns>
    public abstract Task WriteEventAsync(string appCode, string eventName, LogParameter properties,
        Dictionary<string, double> metrics);

    /// <summary>
    ///     Writes a message to this log provider at the <paramref name="level" /> level
    /// </summary>
    /// <param name="level"></param>
    /// <param name="appCode">The identifier for the application or integration</param>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    /// <param name="properties"></param>
    /// <param name="metrics"></param>
    /// <returns></returns>
    public abstract Task LogAsync(LogLevel level, string appCode, string message, LogParameter properties,
        Exception ex = null, Dictionary<string, double> metrics = null);

    /// <summary>
    ///     Releases all managed and unmanaged resources used by the class.
    /// </summary>
    /// <param name="disposing">
    ///     <see cref="bool.TrueString" /> if the method is called from the Dispose() method;
    ///     <see cref="bool.FalseString" /> if the method is called from the finalizer
    /// </param>
    protected abstract void Dispose(bool disposing);
}