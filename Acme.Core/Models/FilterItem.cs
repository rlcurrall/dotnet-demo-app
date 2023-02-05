namespace Acme.Core.Models;

public class FilterItem
{
    public FilterItem(string jsonName, string columnName, Type? propertyType, dynamic value)
    {
        JsonName = jsonName;
        ColumnName = columnName;
        PropertyType = propertyType;
        Value = value;
    }

    public string JsonName { get; set; }

    public string ColumnName { get; set; }

    public Type? PropertyType { get; set; }

    public dynamic Value { get; set; }
}