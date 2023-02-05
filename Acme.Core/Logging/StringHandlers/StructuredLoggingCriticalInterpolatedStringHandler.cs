using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.StringHandlers;

[InterpolatedStringHandler]
public ref struct StructuredLoggingCriticalInterpolatedStringHandler
{
    private readonly StructuredLoggingInterpolatedStringHandler _handler;

    public StructuredLoggingCriticalInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger,
        out bool isEnabled)
    {
        _handler = new StructuredLoggingInterpolatedStringHandler(literalLength, formattedCount, logger,
            LogLevel.Critical, out isEnabled);
    }

    public bool IsEnabled => _handler.IsEnabled;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string value) => _handler.AppendLiteral(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value, [CallerArgumentExpression("value")] string name = "") =>
        _handler.AppendFormatted(value, name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (string, object[]) GetTemplateAndArguments() => _handler.GetTemplateAndArguments();
}