using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Acme.Core.Extensions;

namespace Acme.Core.Attributes;

/// <summary>
///     Attribute that tags parameters on models to dictate what properties can be filtered on.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilterAttribute : ValidationAttribute
{
    public FilterAttribute(string jsonName, string columnName, string? regex = null)
    {
        JsonName = jsonName;
        ColumnName = columnName;
        Regex = regex.IsNullOrWhitespace() ? null : new Regex(regex);
    }

    [Required(ErrorMessage = "JsonName is a required field for a filterable property.")]
    public string JsonName { get; }

    [Required(ErrorMessage = "ColumnName is a required field for a filterable property.")]
    public string ColumnName { get; }

    public Regex? Regex { get; }

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        if (Regex != null)
            return Regex.IsMatch(value.ToString())
                ? ValidationResult.Success
                : new ValidationResult("Input value failed regex validation.");

        return ValidationResult.Success;
    }
}