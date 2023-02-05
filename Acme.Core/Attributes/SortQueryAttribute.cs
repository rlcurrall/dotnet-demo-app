using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Acme.Core.Attributes;

/// <summary>
///     Attribute that validates the incoming query string adheres to the syntax of
///     '?[EntityProperty][:][Direction][,EntityProperty=Direction] (Ex: CustomerName:asc,City=desc)' while also validating
///     any of the fields provided matches the backing entity.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SorterAttribute : ValidationAttribute
{
    private readonly string _invalidInput = "Invalid Direction. Value must be either 'asc' or 'desc'.";

    private readonly Regex _regex = new("((?i)asc|(?i)desc)*");

    public SorterAttribute(string jsonName, string columnName, bool allowNullOrWhitespace = true)
    {
        JsonName = jsonName;
        ColumnName = columnName;
        AllowNullOrWhitespace = allowNullOrWhitespace;
    }

    [Required(ErrorMessage = "JsonName is a required field for a filterable property.")]
    public string JsonName { get; }

    [Required(ErrorMessage = "ColumnName is a required field for a filterable property.")]
    public string ColumnName { get; }

    private bool AllowNullOrWhitespace { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (AllowNullOrWhitespace && string.IsNullOrWhiteSpace(value?.ToString()))
            return ValidationResult.Success;

        return _regex.IsMatch(value.ToString()) ? ValidationResult.Success : new ValidationResult(_invalidInput);
    }
}