using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.LogProviders;

/// <summary>
///     An implementation of the <see cref="BaseLogProvider" /> that writes to Microsoft Application Insights
/// </summary>
internal class AppInsightsLogProvider : BaseLogProvider
{
    private readonly TelemetryClient _mTelemetryClient;

    /// <summary>
    ///     Creates a default instance of <see cref="AppInsightsLogProvider" />.
    /// </summary>
    /// <param name="appInsightsConnection">
    ///     The ConnectionString or InstrumentationKey of the Application Insights instance to
    ///     connect to.
    /// </param>
    public AppInsightsLogProvider(string appInsightsConnection)
    {
        if (string.IsNullOrEmpty(appInsightsConnection))
            throw new ArgumentOutOfRangeException(nameof(appInsightsConnection),
                "The required 'APPINSIGHTS_CONNECTION' key, was either missing or had a null value.");

        // using ProcessExit instead of DomainUnload because DomainUnload is never called
        // for the default application domain, which will be the majority use-case
        // Also, ProcessExit is available in CORE 3.1
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

#pragma warning disable CA2000 // do not want to dispose of the Telemetry Configuration because the TelemetryClient owns it
#pragma warning disable 618 // do not want to dispose of the Telemetry Configuration because the TelemetryClient owns it
        // TelemetryConfiguration properties throw exceptions if they are set to null, so we have to do conditional sets instead of ternary operators
        var config = new TelemetryConfiguration();
        if (appInsightsConnection.IndexOf("InstrumentationKey=", 0, StringComparison.OrdinalIgnoreCase) >= 0)
            config.ConnectionString = appInsightsConnection;
        else
            config.InstrumentationKey = appInsightsConnection;

        _mTelemetryClient = new TelemetryClient(config);

#pragma warning restore 618
#pragma warning restore CA2000
    }

    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        Dispose();
    }

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

        var dict = new Dictionary<string, string>
        {
            {"AppID", appCode}, // the "AppID" property name here is preserved for continuity
            {nameof(properties.Flow), properties.Flow},
            {nameof(properties.Metric1), properties.Metric1},
            {nameof(properties.Metric2), properties.Metric2},
            {nameof(properties.EventType), properties.EventType},
            {nameof(properties.Description), properties.Description},
            {nameof(properties.Direction), properties.Direction},
            {nameof(properties.Domain), properties.Domain},
            {nameof(properties.Stage), properties.Stage},
            {nameof(properties.StageStatus), properties.StageStatus},
            {nameof(properties.Status), properties.Status}
        };

        _mTelemetryClient.TrackEvent(eventName, dict,
            metrics); // schedules the event to be sent. no need to run in an async task
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

        metrics ??= new Dictionary<string, double>();
        var severity = ConvertToSeverity(level);

        var exceptionTel = new ExceptionTelemetry
        {
            Exception = ex,
            Message = message,
            SeverityLevel = severity
        };

        exceptionTel.Properties.Add("AppID", appCode); // the "AppID" property name here is preserved for continuity
        exceptionTel.Properties.Add(nameof(properties.Flow), properties.Flow);
        exceptionTel.Properties.Add(nameof(properties.Metric1), properties.Metric1);
        exceptionTel.Properties.Add(nameof(properties.Metric2), properties.Metric2);
        exceptionTel.Properties.Add(nameof(properties.EventType), properties.EventType);
        exceptionTel.Properties.Add(nameof(properties.Description), properties.Description);
        exceptionTel.Properties.Add(nameof(properties.Direction), properties.Direction);
        exceptionTel.Properties.Add(nameof(properties.Domain), properties.Domain);
        exceptionTel.Properties.Add(nameof(properties.Stage), properties.Stage);
        exceptionTel.Properties.Add(nameof(properties.StageStatus), properties.StageStatus);
        exceptionTel.Properties.Add(nameof(properties.Status), properties.Status);

        foreach (var item in metrics) exceptionTel.Metrics.Add(item);

        _mTelemetryClient.TrackException(exceptionTel);

        return Task.CompletedTask;
    }

    private static SeverityLevel ConvertToSeverity(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                return SeverityLevel.Verbose;
            case LogLevel.Debug:
                return SeverityLevel.Verbose;
            case LogLevel.Information:
                return SeverityLevel.Information;
            case LogLevel.Warning:
                return SeverityLevel.Warning;
            case LogLevel.Error:
                return SeverityLevel.Error;
            case LogLevel.Critical:
                return SeverityLevel.Critical;
            case LogLevel.None:
                return SeverityLevel.Verbose;
            default:
                return SeverityLevel.Information;
        }
    }

    /// <summary>
    ///     Flushes all known telemetry items to all sinks, and waits 2 seconds for the flush to complete.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (MDisposedValue)
            return;

        if (disposing)
        {
            // copy to local variable to make safe for multi-threaded scenarios
            var local = _mTelemetryClient;

            local.Flush(); // flush() is synchronous; when it returns the flushing is done 
        }

        MDisposedValue = true;
    }
}