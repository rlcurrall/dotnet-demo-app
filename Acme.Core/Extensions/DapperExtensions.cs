using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Acme.Core.Attributes;
using Acme.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using static System.String;

namespace Acme.Core.Extensions;

public static class DapperExtensions
{
    private static readonly Regex SortRegex = new("([a-zA-Z0-9_]+)(:{1})((?i)asc|(?i)desc)[,{1}]*");

    /// <summary>
    /// Converts SqlParameters to Dapper DynamicParameters.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static DynamicParameters ToDapperParams(this SqlParameterCollection collection)
    {
        if (collection.Count == 0)
        {
            return new DynamicParameters();
        }

        var dapperParams = new DynamicParameters();

        foreach (SqlParameter sqlParameter in collection)
        {
            dapperParams.Add(sqlParameter.ParameterName, sqlParameter.Value, sqlParameter.DbType);
        }

        return dapperParams;
    }

    /// <summary>
    /// Adds Sorting to a SqlCommand based on a provided query string. Query must be in the following format: '?sort=[EntityProperty][:][Direction][,EntityProperty=Direction] (Ex: CustomerName:asc,City=desc)'
    /// </summary>
    /// <param name="command"></param>
    /// <param name="sorter"></param>
    /// <returns></returns>
    public static SqlCommand AddSorting<T>(this SqlCommand command, T? sorter)
    {
        if (sorter == null || sorter.AllPropertiesAreNullOrEmpty())
        {
            return command;
        }

        //Setup a string builder using the existing query so that we can modify it and set up parameter injection to be sql safe.
        StringBuilder builder = new(command.CommandText);

        builder.AppendLine();
        builder.AppendLine();
        builder.Append("ORDER BY ");

        List<FilterItem> items = new();

        //Get properties and build filter items using the JsonName, backing ColumnName, and Value.
        PropertyInfo[] props = typeof(T).GetProperties();
        foreach (var propertyInfo in props)
        {
            var value = propertyInfo.GetValue(sorter, null);

            if (value != null && !IsNullOrWhiteSpace(value.ToString()))
            {
                items.Add(new FilterItem(propertyInfo.GetCustomAttribute<SorterAttribute>()?.JsonName,
                    propertyInfo.GetCustomAttribute<SorterAttribute>()?.ColumnName,
                    propertyInfo.PropertyType,
                    value));
            }
        }

        var last = items.Last();

        items.ForEach(i =>
        {
            builder.Append(i.Equals(last)
                ? $"{i.ColumnName} {i.Value}"
                : $"{i.ColumnName} {i.Value},");
        });

        //Rebuild our command.
        command.CommandText = builder.ToString();

        //Return the updated command.
        return command;
    }

    /// <summary>
    /// Adds filtering based on the provided filter. Method converts filter object into dynamic sql based on the underlying property types.
    /// </summary>
    /// <typeparam name="T">Filter Object, usually a subset of the database models fields.</typeparam>
    /// <param name="command">Sql Command</param>
    /// <param name="filter">Filter Object</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static SqlCommand AddFiltering<T>(this SqlCommand command, T? filter)
    {
        //If no filter return base command.
        if (filter == null || filter.AllPropertiesAreNullOrEmpty())
        {
            return command;
        }

        //Setup a string builder using the existing query so that we can modify it and set up parameter injection to be sql safe via sql parameters.
        StringBuilder builder = new(command.CommandText);

        builder.AppendLine();
        builder.AppendLine();
        builder.Append("WHERE ");

        List<FilterItem> items = new();

        //Get properties and build filter items using the JsonName, backing ColumnName, and value.
        PropertyInfo[] props = typeof(T).GetProperties();
        foreach (var propertyInfo in props)
        {
            var value = propertyInfo.GetValue(filter, null);

            if (value != null && !IsNullOrWhiteSpace(value.ToString()))
            {
                items.Add(new FilterItem(propertyInfo.GetCustomAttribute<FilterAttribute>().JsonName,
                    propertyInfo.GetCustomAttribute<FilterAttribute>().ColumnName,
                    propertyInfo.PropertyType,
                    value));
            }
        }

        //Snag first property so we know how to setup the string.
        var firstProperty = items.First();

        foreach (var item in items)
        {
            //Check to see if the property type is optional. If it is then the type will be of Nullable<T>. Since we cannot switch on Nullable<T> then we get the underlying type and set the switch value to that.
            Type? switchType;
            if (item.PropertyType is {IsGenericType: true} &&
                item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                switchType = Nullable.GetUnderlyingType(item.PropertyType);
            }
            else
            {
                switchType = item.PropertyType;
            }

            //Switch based on property type
            switch (switchType)
            {
                case { } when switchType == typeof(string):

                    SqlParameter stringProp = new()
                    {
                        ParameterName = item.ColumnName,
                        Value = (string) item.Value,
                        DbType = DbType.String
                    };

                    command.Parameters.Add(stringProp);

                    builder.Append(item.Equals(firstProperty)
                        ? $"{stringProp.ParameterName} = :{stringProp.ParameterName}"
                        : $"AND {stringProp.ParameterName} = :{stringProp.ParameterName}");

                    break;

                case { } when switchType == typeof(int):
                    SqlParameter intProp = new()
                    {
                        ParameterName = item.ColumnName,
                        Value = (int) item.Value,
                        DbType = DbType.Int32
                    };

                    command.Parameters.Add(intProp);

                    builder.Append(item.Equals(firstProperty)
                        ? $"{intProp.ParameterName} = :{intProp.ParameterName}"
                        : $"AND {intProp.ParameterName} = :{intProp.ParameterName}");

                    break;

                case { } dateType when dateType == typeof(DateTime):
                    SqlParameter dateProp = new()
                    {
                        ParameterName = item.ColumnName,
                        Value = (DateTime) item.Value,
                        DbType = DbType.DateTime
                    };

                    command.Parameters.Add(dateProp);

                    builder.Append(item.Equals(firstProperty)
                        ? $"{dateProp.ParameterName} = :{dateProp.ParameterName}"
                        : $"AND {dateProp.ParameterName} = :{dateProp.ParameterName}");

                    break;

                default:
                    throw new Exception(
                        "An unsupported property type was encountered when converting filter query to sql. Only strings, integers, and datetimes are supported at this time.");
            }

            builder.AppendLine();
        }

        //Update our command text.
        command.CommandText = builder.ToString();

        //Return the updated command.
        return command;
    }

    public static SqlCommand AddSnowflakeOffsetLimit(this SqlCommand command, OffsetLimit offsetLimit)
    {
        //Setup a string builder using the existing query so that we can modify it and set up parameter injection to be sql safe.
        StringBuilder builder = new(command.CommandText);

        builder.AppendLine();
        builder.AppendLine();
        builder.Append($"OFFSET {offsetLimit.Offset} ROWS FETCH NEXT {offsetLimit.Limit} ROWS ONLY");

        //Update our command text.
        command.CommandText = builder.ToString();

        //Return the updated command.
        return command;
    }

    public static SqlCommand AddSqlOffsetLimit(this SqlCommand command, int offset, int next)
    {
        var stringBuilder = new StringBuilder(command.CommandText);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        var stringBuilder2 = stringBuilder;
        var handler =
            new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
        handler.AppendLiteral("OFFSET ");
        handler.AppendFormatted(offset);
        handler.AppendLiteral(" ROWS FETCH NEXT ");
        handler.AppendFormatted(next);
        handler.AppendLiteral(" ROWS ONLY");
        stringBuilder2.Append(ref handler);
        command.CommandText = stringBuilder.ToString();
        return command;
    }

    public static SqlCommand AddSqlSorting(this SqlCommand command, string? sort, Type type)
    {
        if (sort.IsNullOrWhitespace())
        {
            StringBuilder nullStringBuilder = new(command.CommandText);

            nullStringBuilder.AppendLine();
            nullStringBuilder.AppendLine();
            nullStringBuilder.Append("ORDER BY (SELECT NULL)");

            command.CommandText = nullStringBuilder.ToString();
            return command;
        }

        StringBuilder stringBuilder = new(command.CommandText);

        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        stringBuilder.Append("ORDER BY ");

        Dictionary<string, string> dictionary = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> item in (from p in type.GetProperties()
                     where p.IsDefined(typeof(FilterableAttribute), inherit: false) &&
                           p.IsDefined(typeof(JsonPropertyAttribute), inherit: false)
                     select p).ToDictionary(
                     (PropertyInfo p) => p.GetCustomAttribute<JsonPropertyAttribute>()!.PropertyName,
                     (PropertyInfo p) => p.Name))
        {
            item.Deconstruct(out var key, out var value);
            var key2 = key;
            var value2 = value;
            dictionary.Add(key2, value2);
        }

        if (sort != null)
        {
            var matchCollection = SortRegex.Matches(sort);
            var obj = matchCollection.Last();
            foreach (Match item2 in matchCollection)
            {
                if (item2.Equals(obj))
                {
                    var stringBuilder2 = stringBuilder;
                    var stringBuilder3 = stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler = new(1, 2, stringBuilder2);
                    handler.AppendFormatted(dictionary[item2.Groups[1].Value]);
                    handler.AppendLiteral(" ");
                    handler.AppendFormatted(item2.Groups[3].Value);
                    stringBuilder3.Append(ref handler);
                }
                else
                {
                    var stringBuilder2 = stringBuilder;
                    var stringBuilder4 = stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler = new(2, 2, stringBuilder2);
                    handler.AppendFormatted(dictionary[item2.Groups[1].Value]);
                    handler.AppendLiteral(" ");
                    handler.AppendFormatted(item2.Groups[3].Value);
                    handler.AppendLiteral(",");
                    stringBuilder4.Append(ref handler);
                }

                stringBuilder.AppendLine();
            }
        }

        command.CommandText = stringBuilder.ToString();
        return command;
    }
}