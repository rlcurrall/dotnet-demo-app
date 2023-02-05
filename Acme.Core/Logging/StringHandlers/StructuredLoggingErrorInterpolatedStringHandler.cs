using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.StringHandlers;

[InterpolatedStringHandler]
public ref struct StructuredLoggingErrorInterpolatedStringHandler
{
    private readonly StructuredLoggingInterpolatedStringHandler _handler;

    public StructuredLoggingErrorInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger,
        out bool isEnabled)
    {
        _handler = new StructuredLoggingInterpolatedStringHandler(literalLength, formattedCount, logger, LogLevel.Error,
            out isEnabled);
    }

    public bool IsEnabled => _handler.IsEnabled;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string s) => _handler.AppendLiteral(s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value, [CallerArgumentExpression("value")] string name = "") =>
        _handler.AppendFormatted(value, name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (string, object?[]) GetTemplateAndArguments() => _handler.GetTemplateAndArguments();
}