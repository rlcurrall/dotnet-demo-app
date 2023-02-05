namespace Acme.Core.Extensions;

public static class DateTimeExtensions
{
    public static bool Between(this DateTime date, DateTime minDate, DateTime maxDate, bool inclusive = false)
    {
        return inclusive
            ? minDate <= date && date <= maxDate
            : minDate < date && date < maxDate;
    }

    public static bool Between(this DateTime date, DateTime? minDate, DateTime? maxDate, bool inclusive = false)
    {
        if (minDate != null)
        {
            if (maxDate != null) return date.Between(minDate, maxDate, inclusive);

            return inclusive ? date >= minDate : date > minDate;
        }

        if (maxDate != null) return inclusive ? date <= maxDate : date < maxDate;

        return inclusive
            ? minDate <= date && date <= maxDate
            : minDate < date && date < maxDate;
    }
}