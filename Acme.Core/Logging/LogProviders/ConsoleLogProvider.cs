using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.LogProviders;

internal class ConsoleLogProvider : BaseLogProvider
{
    public override Task WriteEventAsync(string appCode, string eventName, LogParameter properties,
        Dictionary<string, double> metrics)
    {
        if (string.IsNullOrWhiteSpace(appCode))
            throw new ArgumentNullException(nameof(appCode));
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentNullException(nameof(eventName));
        if (properties == null)
            throw new ArgumentNullException(nameof(properties));

        metrics ??= new Dictionary<string, double>();

        Console.WriteLine("{0} {1} {2} {3}", DateTime.Now.ToString(TimestampFormat), appCode,
            ConvertToLogString(LogLevel.Critical), eventName);
        foreach (KeyValuePair<string, double> item in metrics) Console.WriteLine((item.Key ?? "") + ": " + item.Value);

        return Task.CompletedTask;
    }

    public override Task LogAsync(LogLevel level, string appCode, string message, LogParameter properties,
        Exception ex = null, Dictionary<string, double> metrics = null)
    {
        if (string.IsNullOrWhiteSpace(appCode))
            throw new ArgumentOutOfRangeException(nameof(appCode), "Cannot be null or whitespace");
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (properties == null)
            throw new ArgumentNullException(nameof(properties));

        Console.WriteLine("{0} {1} {2} {3}", DateTime.Now.ToString(TimestampFormat), appCode, ConvertToLogString(level),
            message);

        if (ex != null)
            Console.WriteLine(ex.ToString());

        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ConvertToLogString(LogLevel l)
    {
        switch (l)
        {
            case LogLevel.Critical:
                return "CRIT ";
            case LogLevel.Debug:
                return "DEBUG";
            case LogLevel.Error:
                return "ERROR";
            case LogLevel.Information:
                return "INFO ";
            case LogLevel.Trace:
                return "TRACE";
            case LogLevel.Warning:
                return "WARN ";
            case LogLevel.None:
                return "NONE ";
        }

        return "UNKNO";
    }

    protected override void Dispose(bool disposing)
    {
        MDisposedValue = true;
        // nothing else to do
    }
}