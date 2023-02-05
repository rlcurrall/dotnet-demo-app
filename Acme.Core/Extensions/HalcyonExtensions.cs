using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Acme.Core.Models.Halcyon;
using Halcyon.HAL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static System.Int32;
using static System.String;

namespace Acme.Core.Extensions;

public static class HalcyonExtensions
{
    /// <summary>
    /// Generates Halcyon Urls with updated query strings to reflect offset and limit where applicable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TU"></typeparam>
    /// <param name="halObject"></param>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Link[] BuildLinks<T, TU>(this T halObject, HttpContext context, ILogger<TU> logger)
        where T : HalcyonBase
    {
        StringBuilder builder = new();
        List<Link> links = new();

        var route = context.Request.Path.Value;
        var query = context.Request.QueryString.Value;

        var offsetParse = TryParse(HttpUtility.ParseQueryString(query ?? Empty).Get("Offset"), out var offset);
        var limitParse = TryParse(HttpUtility.ParseQueryString(query ?? Empty).Get("Limit"), out var limit);

        if (!offsetParse || !limitParse)
        {
            logger.LogError("Unable to parse Offset and Limit Query Parameters");
            return links.ToArray();
        }

        builder.Append(query);

        //Add previous link only if we are not starting at the beginning of the result set
        if (offset > 1)
        {
            //If offset - limit is greater than 0 then we know we can do a full offset
            if (offset - limit >= 0)
            {
                var updatedQuery = builder.Replace("&Offset=" + offset, "&Offset=" + (offset - limit)).ToString();
                Link prev = new("prev", $"{route}{updatedQuery}");
                links.Add(prev);
            }
            //Otherwise we set the offset to 0
            else
            {
                var updatedQuery = builder.Replace("&Offset=" + offset, "&Offset=" + 0).ToString();
                Link prev = new("prev", $"{route}{updatedQuery}");
                links.Add(prev);
            }
        }

        builder.Clear();
        builder.Append(query);

        //Always add self link
        Link self = new("self", $"{route}{builder}");
        links.Add(self);

        builder.Clear();
        builder.Append(query);

        //Add next only if there are more results that can be grabbed
        if (offset + limit < halObject.TotalCount)
        {
            var updatedQuery = builder.Replace("&Offset=" + offset, "&Offset=" + (offset + limit)).ToString();
            Link next = new("next", $"{route}{updatedQuery}");
            links.Add(next);
        }

        return links.ToArray();
    }

    [Obsolete("Please utilize the newer BuildLinks extension method.")]
    public static Link[] BuildPagingLinks<T>(this T halObject, string route, int page, int pageCount,
        params KeyValuePair<string, string>[] queryArgs) where T : HalcyonBase
    {
        List<Link> links = new();

        StringBuilder queryBuilder = new(Empty);

        //Optional Query Args if provided
        if (queryArgs.Any())
        {
            foreach (var pair in queryArgs)
            {
                if (!pair.Value.IsNullOrWhitespace())
                {
                    queryBuilder.Append($"&{pair.Key}={pair.Value}");
                }
            }
        }

        var query = queryBuilder.ToString();

        //Add previous only if on a page greater than 1
        if (page > 1)
        {
            Link prev = new("prev", $"{route}?page={page - 1}&pageCount={pageCount}{query}");
            links.Add(prev);
        }

        //Always add self link
        Link self = new("self", $"{route}?page={page}&pageCount={pageCount}{query}");
        links.Add(self);

        //Add next only if there are more results that can be grabbed
        if (page * pageCount < halObject.TotalCount)
        {
            Link next = new("next", $"{route}?page={(page.Equals(null) ? 2 : page + 1)}&pageCount={pageCount}{query}");
            links.Add(next);
        }

        return links.ToArray();
    }

    public static Link[] BuildOffsetLinks<T>(this T halObject, string route, int offset, int limit,
        params KeyValuePair<string, string>[] queryArgs) where T : HalcyonBase
    {
        List<Link> list = new();
        StringBuilder stringBuilder = new(Empty);

        if (queryArgs.Any())
        {
            foreach (var keyValuePair in queryArgs)
            {
                if (keyValuePair.Value.IsNullOrWhitespace()) continue;
                var stringBuilder2 = stringBuilder;
                StringBuilder.AppendInterpolatedStringHandler handler = new(2, 2, stringBuilder2);
                handler.AppendLiteral("&");
                handler.AppendFormatted(keyValuePair.Key);
                handler.AppendLiteral("=");
                handler.AppendFormatted(keyValuePair.Value);
                stringBuilder2.Append(ref handler);
            }
        }

        var value = stringBuilder.ToString();
        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;

        if (offset > limit)
        {
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 4);
            defaultInterpolatedStringHandler.AppendFormatted(route);
            defaultInterpolatedStringHandler.AppendLiteral("?offset=");
            defaultInterpolatedStringHandler.AppendFormatted(offset - (limit + 1));
            defaultInterpolatedStringHandler.AppendLiteral("&limit=");
            defaultInterpolatedStringHandler.AppendFormatted(limit);
            defaultInterpolatedStringHandler.AppendFormatted(value);
            Link item = new("prev", defaultInterpolatedStringHandler.ToStringAndClear());
            list.Add(item);
        }

        defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 4);
        defaultInterpolatedStringHandler.AppendFormatted(route);
        defaultInterpolatedStringHandler.AppendLiteral("?offset=");
        defaultInterpolatedStringHandler.AppendFormatted(offset);
        defaultInterpolatedStringHandler.AppendLiteral("&limit=");
        defaultInterpolatedStringHandler.AppendFormatted(limit);
        defaultInterpolatedStringHandler.AppendFormatted(value);
        Link item2 = new("self", defaultInterpolatedStringHandler.ToStringAndClear());
        list.Add(item2);

        if (offset >= halObject.TotalCount) return list.ToArray();
        defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 4);
        defaultInterpolatedStringHandler.AppendFormatted(route);
        defaultInterpolatedStringHandler.AppendLiteral("?offset=");
        defaultInterpolatedStringHandler.AppendFormatted(offset + (limit + 1));
        defaultInterpolatedStringHandler.AppendLiteral("&limit=");
        defaultInterpolatedStringHandler.AppendFormatted(limit);
        defaultInterpolatedStringHandler.AppendFormatted(value);
        Link item3 = new("next", defaultInterpolatedStringHandler.ToStringAndClear());
        list.Add(item3);

        return list.ToArray();
    }
}