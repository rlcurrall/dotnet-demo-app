namespace Acme.Core.Logging;

/// <summary>
///     Model class for Log parameters
/// </summary>
public class LogParameter
{
    /// <summary>
    ///     preferably with milliseconds or up to seconds
    /// </summary>
    [Obsolete(
        "This property is ignored by the Acme.Core.Log class. the Log Timestamp format is defined by BaseLogProvider.TimestampFormat")]
    public string TimeStamp { get; set; }

    /// <summary>
    ///     DEBUG, ERROR, INFO, FATAL
    /// </summary>
    [Obsolete(
        "This property is ignored. Either specify the LogLevel with a direct call to BaseLogProvider.Log() or use one of the Log.LogX() methods")]
    public string LogLevel { get; set; }

    /// <summary>
    ///     See the CIRRUS SharePoint site for the details.
    /// </summary>
    [Obsolete("This property is ignored. Specify the AppId when adding a log provider instead")]
    public string AppID { get; set; }

    /// <summary>
    ///     CUSTOMER, MATERIAL, VENDOR, DAILY SALES, etc…
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    ///     See the CIRRUS SharePoint site for the details.
    /// </summary>
    public string Flow { get; set; }

    /// <summary>
    ///     API, APIM, STOR PROC, SSIS, AUDIT LOG, etc…
    /// </summary>
    public string Stage { get; set; }

    /// <summary>
    ///     IN, OUT
    /// </summary>
    public string Direction { get; set; }

    /// <summary>
    ///     ADD, UPDATE, DELETE
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    ///     SUCCESS, FAILURE
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    ///     a custom description of the log event
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     START, END
    /// </summary>
    public string StageStatus { get; set; }

    /// <summary>
    ///     Metric1 and Metric2– It’s a place holder for any additional data for logging. Ex: Cust #, Material #, URL,
    ///     NumberOfRecordsProcessed, NumberOfRecordsReceived, etc…
    /// </summary>
    public string Metric1 { get; set; }

    /// <summary>
    ///     Metric1 and Metric2– It’s a place holder for any additional data for logging. Ex: Cust #, Material #, URL,
    ///     NumberOfRecordsProcessed, NumberOfRecordsReceived, etc…
    /// </summary>
    public string Metric2 { get; set; }

    /// <summary>
    ///     Creates a deep copy of this <see cref="LogParameter" />
    /// </summary>
    /// <returns>A deep copy of this <see cref="LogParameter" /></returns>
    public LogParameter Copy()
    {
        return new LogParameter
        {
            Domain = Metric2,
            Flow = Flow,
            Stage = Stage,
            Direction = Direction,
            EventType = EventType,
            Status = Status,
            Description = Description,
            StageStatus = StageStatus,
            Metric1 = Metric1,
            Metric2 = Metric2
        };
    }
}