using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Acme.Core.Extensions;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Acme.Core.Attributes;

/// <summary>
///     Attribute that validates the incoming query string adheres to the syntax of
///     '?filter=[EntityProperty][=][Value][[,][EntityProperty][=][Value]] (Ex: CustomerName=TestName,City=TestCity)'
///     while also validating any of the fields provided match the backing entity and are allowed to be filtered on.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FilterQueryAttribute : ValidationAttribute, IBindingSourceMetadata, IModelNameProvider, IFromQueryMetadata
{
    private readonly string _erroneousProperty =
        "Sort query contains properties that do not exist on the entity. Please ensure the following property is valid on the entity being queried: {0}";

    private readonly string _invalidQuery =
        "Filter query is not structured properly. Please ensure query parameters are structured as follows: '?filter=[EntityProperty][=][Value][[,][EntityProperty][=][Value]] (Ex: CustomerName=TestName,City=Greenville)";

    private readonly string _nonfilterableProperty =
        "Filter query contains a filter property that cannot be filtered on the entity. Invalid property: {0}";

    private readonly Regex _regex = new("([a-zA-Z0-9_]+)(={1})([a-zA-Z0-9_ ]*)[,{1}]*"); //Hello regex my old friend

    public FilterQueryAttribute(Type propertyType)
    {
        PropertyType = propertyType;
    }

    private Type PropertyType { get; }

    public BindingSource BindingSource => BindingSource.Query;

    public string? Name { get; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        //If filter is present match against the above regex.
        if ((string) value != string.Empty)
        {
            var query = (string) value;

            //If we pass the regex then we need to breakup the results into capture groups and ensure that all of the properties match the properties that exist on the entity and that those properties are tagged with the filterable attribute.
            if (_regex.IsMatch(query))
            {
                //We want to use the JsonPropertyAttribute name as that is what is exposed to the end user. This will be converted on the repo side to the true entity field name.
                List<string?> allProperties = PropertyType.GetProperties().Select(p => p
                    .GetCustomAttribute<JsonPropertyAttribute>()
                    .PropertyName).ToList();

                List<string?> filterableProperties = PropertyType.GetProperties()
                    .Where(p => p.IsDefined(typeof(FilterableAttribute), false) &&
                                p.IsDefined(typeof(JsonPropertyAttribute), false)).Select(p => p
                        .GetCustomAttribute<JsonPropertyAttribute>()
                        .PropertyName).ToList();

                var matches = _regex.Matches(query);

                foreach (Match match in matches)
                {
                    //Check first to see if the property exists.
                    if (!allProperties.Contains(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
                        return new ValidationResult(string.Format(_erroneousProperty, match.Groups[1].Value));

                    //Check second to ensure that it is a valid property to be filtered on.
                    if (!filterableProperties.Contains(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase))
                        return new ValidationResult(string.Format(_nonfilterableProperty, match.Groups[1].Value));
                }
            }
            //If sort fails regex throw a validation failure.
            else
            {
                return new ValidationResult(_invalidQuery);
            }
        }

        return ValidationResult.Success;
    }
}