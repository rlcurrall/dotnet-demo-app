namespace Acme.Core.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SwaggerIgnoreAttribute : Attribute
{
}