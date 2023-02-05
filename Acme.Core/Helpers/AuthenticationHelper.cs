using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Helpers;

public static class AuthenticationHelper
{
    public static string ParseTokenHeaderValue<T>(string token, T logger) where T : ILogger
    {
        try
        {
            if (!AuthenticationHeaderValue.TryParse(token, out var headerValue)) return string.Empty;
            // If we have a valid AuthenticationHeaderValue we will have the following details:
            // Scheme will be "Bearer"
            // Parameter will be the token itself.
            var scheme = headerValue.Scheme; //Ignore for now
            var parameter = headerValue.Parameter;

            return parameter;

        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
            return string.Empty;
        }
    }
}