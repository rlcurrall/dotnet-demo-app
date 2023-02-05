using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Acme.Core.Filters;

/// <summary>
///     Class for adding additional Swagger Configurations. Currently enables the following:
///     EnumSchemaFilter -> Enum Names for the associated values.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        schema.Enum.Clear();
        Enum.GetNames(context.Type)
            .ToList()
            .ForEach(name =>
                schema.Enum.Add(
                    new OpenApiString($"{Convert.ToInt64(Enum.Parse(context.Type, name))} - {name}")));
    }
}