namespace Acme.Core.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhitespace(this string? s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// Checks and returns true if any string property on the objects is null or empty.
    /// </summary>
    /// <param name="myObject"></param>
    /// <returns></returns>
    public static bool IsAnyNullOrEmpty(this object myObject)
    {
        foreach (var pi in myObject.GetType().GetProperties())
        {
            if (pi.PropertyType == typeof(string))
            {
                var value = (string) pi.GetValue(myObject)!;
                if (value.IsNullOrEmpty())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool AllPropertiesAreNullOrEmpty(this object myObject)
    {
        foreach (var pi in myObject.GetType().GetProperties())
        {
            dynamic value = pi.GetValue(myObject) ?? null;

            if (value != null) return false;
        }

        return true;
    }

    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source.Contains(toCheck, comp);
    }

    public static bool Contains(this IEnumerable<string> source, string toCheck, StringComparison comp)
    {
        return source.Any(s => s.Contains(toCheck, comp));
    }
}