using System.Reflection;
using Acme.Core.Attributes;
using Acme.Core.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Acme.Core.Filters;

public class SwaggerIgnoreFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext schemaFilterContext)
    {
        if (schema.Properties.Count == 0)
            return;

        const BindingFlags bindingFlags = BindingFlags.Public |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance;

        IEnumerable<MemberInfo> memberList = schemaFilterContext.Type // In v5.3.3+ use Type instead
            .GetFields(bindingFlags).Cast<MemberInfo>()
            .Concat(schemaFilterContext.Type // In v5.3.3+ use Type instead
                .GetProperties(bindingFlags));

        IEnumerable<string> excludedList = memberList.Where(m =>
                m.GetCustomAttribute<SwaggerIgnoreAttribute>()
                != null)
            .Select(m =>
                m.GetCustomAttribute<JsonPropertyAttribute>()
                    ?.PropertyName
                ?? m.Name.ToCamelCase());

        foreach (var excludedName in excludedList)
            if (schema.Properties.ContainsKey(excludedName))
                schema.Properties.Remove(excludedName);
    }
}