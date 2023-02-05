namespace Acme.Core.Extensions;

public static class NumericExtensions
{
    public static bool Between(this long num, long lower, long upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }

    public static bool Between(this long num, long? lower, long? upper, bool inclusive = false)
    {
        if (lower != null)
        {
            if (upper != null)
            {
                return num.Between(lower, upper, inclusive);
            }

            return inclusive ? num >= lower : num > lower;
        }

        if (upper != null)
        {
            return inclusive ? num <= upper : num < upper;
        }

        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }
}