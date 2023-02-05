using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging.StringHandlers;

[InterpolatedStringHandler]
public ref struct StructuredLoggingInterpolatedStringHandler
{
    private readonly StringBuilder _template = null!;
    private readonly ArgumentList _arguments = null!;

    public bool IsEnabled { get; }

    public StructuredLoggingInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger,
        LogLevel logLevel, out bool isEnabled)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        IsEnabled = isEnabled = logger.IsEnabled(logLevel);
        if (isEnabled)
        {
            _template = new(literalLength + (20 * formattedCount));
            _arguments = new(formattedCount);
        }
    }

    public readonly void AppendLiteral(string value)
    {
        if (!IsEnabled)
            return;

        _template.Append(
            value.Replace("{", "{{", StringComparison.Ordinal).Replace("}", "}}", StringComparison.Ordinal));
    }

    public readonly void AppendFormatted<T>(T value, [CallerArgumentExpression("value")] string name = "")
    {
        if (!IsEnabled)
            return;
        if (name.StartsWith("@"))
        {
            name = name[1..];
            _arguments.Add(JsonSerializer.Serialize(value));
        }
        else
        {
            _arguments.Add(value);
        }

        _template.Append($"{{{name}}}");
    }

    public readonly (string, object[]) GetTemplateAndArguments() => (_template?.ToString(), _arguments?.Arguments);

    private class ArgumentList
    {
        private int _index;

        public object[] Arguments { get; }

        public ArgumentList(int formattedCount) => Arguments = new object[formattedCount];

        public void Add(object value) => Arguments[_index++] = value;
    }
}