using System.Reflection;
using System.Text;

namespace Acme.Core.Extensions;

public static class QueryStringExtensions
{
    private static readonly StringBuilder QueryBuilder = new();

    /// <summary>
    ///     Converts a provided object to a query string. Does allow nested objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToQueryString<T>(this T obj) where T : class
    {
        QueryBuilder.Clear();

        BuildQueryString(obj);

        if (QueryBuilder.Length > 0)
            QueryBuilder[0] = '?';

        return QueryBuilder.ToString();
    }

    private static void BuildQueryString<T>(T? obj, string prefix = "") where T : class
    {
        if (obj == null)
            return;

        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (property.GetValue(obj, Array.Empty<object>()) != null)
            {
                var value = property.GetValue(obj, Array.Empty<object>());

                switch (property.PropertyType.IsArray)
                {
                    case true when value?.GetType() == typeof(DateTime[]):
                    {
                        foreach (var item in (DateTime[]) value)
                            QueryBuilder.Append($"&{prefix}{property.Name}={item.ToString("yyyy-MM-dd")}");
                        break;
                    }
                    case true:
                    {
                        foreach (var item in (Array) value!)
                            QueryBuilder.Append($"&{prefix}{property.Name}={item}");
                        break;
                    }
                    default:
                    {
                        if (property.PropertyType == typeof(string))
                            QueryBuilder.Append($"&{prefix}{property.Name}={value}");

                        else if (property.PropertyType == typeof(DateTime) &&
                                 !value!.Equals(Activator.CreateInstance(property.PropertyType))) // is not default 
                            QueryBuilder.Append(
                                $"&{prefix}{property.Name}={((DateTime) value).ToString("yyyy-MM-dd")}");

                        else if (property.PropertyType.IsValueType &&
                                 !value!.Equals(Activator.CreateInstance(property.PropertyType))) // is not default 
                            QueryBuilder.Append($"&{prefix}{property.Name}={value}");

                        else if (property.PropertyType.IsClass)
                            BuildQueryString(value, $"{prefix}{property.Name}.");
                        break;
                    }
                }
            }
    }
}