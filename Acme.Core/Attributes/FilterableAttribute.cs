using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Acme.Core.Extensions;

namespace Acme.Core.Attributes;

/// <summary>
///     Attribute that tags parameters on models to dictate what properties can be filtered on.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterableAttribute : ValidationAttribute
{
    private readonly string _badDatetime = "DateTime must be between the following dates: {0} - {1}";

    private readonly string _badInt = "Integer must be between the following range: {0} - {1}";

    private readonly string _badString = "String must be between the following length: {0} - {1}";

    //Handful of the most common date times. Update as needed for new formats
    private readonly string[] _dateFormats =
    {
        "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyy", "yyyy/MM/dd", "yyyy-MM-dd", "MM/dd/yyyy HH:mm",
        "MM/dd/yyyy hh:mm tt", "dd-MM-yyyy HH:mm", "dd/MM/yyyy hh:mm tt", "MM-dd-yyyy HH:mm", "MM-dd-yyyy hh:mm tt",
        "yyyy-MM-dd HH:mm", "yyyy/MM/dd hh:mm tt"
    };

    private readonly string _unparseableDate =
        "Unable to parse the following date: {0}. The allowed DateTime formats are as follows: {1}";

    /// <summary>
    ///     Tag for allowing filtering on this property.
    /// </summary>
    public FilterableAttribute()
    {
    }

    /// <summary>
    ///     Tag for defining numeric ranges or string lengths.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="inclusive"></param>
    public FilterableAttribute(Type type, int lower = int.MinValue, int upper = int.MaxValue, bool inclusive = false)
    {
        Type = type;
        Lower = lower;
        Upper = upper;
        Inclusive = inclusive;
    }

    /// <summary>
    ///     Tag for defining DateTime ranges.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <param name="inclusive"></param>
    public FilterableAttribute(Type type, string minDate, string maxDate, bool inclusive = false)
    {
        Type = type;
        MinDate = minDate;
        MaxDate = maxDate;
        Inclusive = inclusive;
    }

    private Type? Type { get; }

    private int Lower { get; }

    private int Upper { get; }

    private bool Inclusive { get; }

    private string MinDate { get; }

    private string MaxDate { get; }

    //Fine Italian cuisine below. Since we are not passing in the type via the query string we do some pattern matching and dynamically find the most accurate type.
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        //Setup here for future expansion
        DateTime minDate;
        DateTime maxDate;
        bool stringBetween;
        bool intBetween;
        bool dateBetween;

        switch (Type)
        {
            //Case when no specific actions are applied on top of the object.
            case { } nullType when nullType == null:
                return ValidationResult.Success;

            //String length checking
            case { } stringType when stringType == typeof(string):

                stringBetween = ((long) ((string) value).Length).Between(Lower, Upper, Inclusive);

                if (!stringBetween)
                    return new ValidationResult(string.Format(_badString, Lower.ToString(), Upper.ToString()));

                return ValidationResult.Success;

            //Int range checking. Break up in the future? TODO...
            case { } shortType when shortType == typeof(short):
            case { } intType when intType == typeof(int):
            case { } longType when longType == typeof(long):

                intBetween = ((long) value).Between(Lower, Upper, Inclusive);

                if (!intBetween)
                    return new ValidationResult(string.Format(_badInt, Lower.ToString(), Upper.ToString()));

                return ValidationResult.Success;

            //DateTime range checking
            case { } dateType when dateType == typeof(DateTime):

                if (MinDate.IsNullOrWhitespace())
                {
                    minDate = DateTime.MinValue;
                }
                else
                {
                    if (DateTime.TryParseExact(MinDate, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out minDate))
                    {
                        //Var updated so we do nothing.
                    }
                    else
                    {
                        return new ValidationResult(string.Format(_unparseableDate, $"MinDate: {MinDate}",
                            string.Join(", ", _dateFormats)));
                    }
                }

                if (MaxDate.IsNullOrWhitespace())
                {
                    maxDate = DateTime.MaxValue;
                }
                else
                {
                    if (DateTime.TryParseExact(MaxDate, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out maxDate))
                    {
                        //Var updated so we do nothing.
                    }
                    else
                    {
                        return new ValidationResult(string.Format(_unparseableDate, $"MaxDate: {MaxDate}",
                            string.Join(", ", _dateFormats)));
                    }
                }

                dateBetween = ((DateTime) value).Ticks.Between(minDate.Ticks, maxDate.Ticks, Inclusive);

                if (!dateBetween)
                    return new ValidationResult(string.Format(_badDatetime,
                        MinDate.ToString(CultureInfo.InvariantCulture),
                        MaxDate.ToString(CultureInfo.InvariantCulture)));

                return ValidationResult.Success;

            default:
                return ValidationResult.Success;
        }
    }
}