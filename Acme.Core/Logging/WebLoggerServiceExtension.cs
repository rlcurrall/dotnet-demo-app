using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Logging;

public static class WebLoggerServiceExtension
{
    public static IApplicationBuilder UseWebLogContextMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LogContextMiddleware>();
    }

    /// https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
    /// Important: Connection Strings are recommended over instrumentation keys.New Azure regions require using connection strings
    /// instead of instrumentation keys. Connection string identifies the resource that you want to associate your telemetry data with.
    /// It also allows you to modify the endpoints your resource will use as a destination for your telemetry.
    /// You will need to copy the connection string and add it to your application's code or to an environment variable.
    public static IServiceCollection AddWebLogger(
        this IServiceCollection services, WebLoggerOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        if (!options.IsValid()) throw new ArgumentException("AppInsights Connection String Or App Id is invalid");

        services.AddSingleton(options);
        ApplicationInsightsServiceOptions insightsServiceOptions =
            new() {ConnectionString = options.AppInsightsConnectionString};
        services.AddApplicationInsightsTelemetry(insightsServiceOptions);
        services.AddSingleton(typeof(ILogger<>), typeof(WebLogger<>));
        return services;
    }
}