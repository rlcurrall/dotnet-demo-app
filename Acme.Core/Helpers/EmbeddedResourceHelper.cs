using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Acme.Core.Helpers;

public class EmbeddedResourceHelper
{
    /// <summary>
    ///     Helper method for retrieving embedded sql scripts and json data. IE:
    ///     GetEmbeddedResource("Acme.Api.Weather.json"). Returns a SqlCommand that can be chained together
    ///     with AddQueries();
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static SqlCommand GetEmbeddedResource(string assemblyPath)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var script = callingAssembly.GetManifestResourceStream(assemblyPath);

        if (script == null)
            throw new NullReferenceException(
                $"Unable to find embedded resource {assemblyPath} in assembly: {callingAssembly.FullName}");

        using StreamReader reader = new(script);

        SqlCommand command = new()
        {
            CommandText = reader.ReadToEnd(),
            CommandType = CommandType.Text
        };

        return command;
    }
}