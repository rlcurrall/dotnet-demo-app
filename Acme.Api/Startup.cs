using Acme.Data.Interfaces;
using Acme.Data.Repositories;
using Acme.Api.Config;
using Acme.Core.Enums;
using Acme.Core.Extensions;
using Acme.Core.Logging;
using Acme.Services.Interfaces;
using Acme.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Acme.Api;

public class Startup
{
    private readonly string _appInsightsConnectionString;

    private readonly string _appName;

    private readonly string _appVersion;

    //Add Roles here for testing
    private readonly Dictionary<string, string> _scopes = new();

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;

        _appName = Configuration.GetValue<string>(Constants.AppName);
        _appVersion = Configuration.GetValue<string>(Constants.AppVersion);

        _appInsightsConnectionString = Configuration.GetValue<string>(Constants.AppInsightsConnectionString);
    }

    public IConfiguration Configuration { get; set; }

    public IWebHostEnvironment Environment { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        //currently running against appin-sbx-api-poc
        services.AddWebLogger(new WebLoggerOptions
        {
            AppId = $"{_appName} - {_appVersion}",
            AppInsightsConnectionString = _appInsightsConnectionString
        });

        //Set CORS Configuration
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.SetIsOriginAllowed(origin => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        //Inherently check against Azure AD to make sure the request has a valid token before allowing the request in.
        //services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

        services.AddControllers(options => options.EnableEndpointRouting = false).AddNewtonsoftJson();

        services.AddMemoryCache();

        #region Services

        services.AddScoped<IWeatherService, WeatherService>();

        #endregion

        #region HttpServices

        //.AddHttpClient();

        #endregion

        #region Repositories

        services.AddScoped<IWeatherRepository, WeatherRepository>();

        #endregion

        //http://localhost:51234/swagger/V1/swagger.json
        services.GenerateSwagger(Configuration, _scopes, AuthenticationType.None);
    }

    public async void Configure(IApplicationBuilder appBuilder)
    {
        if (Environment.IsDevelopment()) appBuilder.UseDeveloperExceptionPage();

        appBuilder.UseWebLogContextMiddleware();

        appBuilder.UseSwagger(Configuration, AuthenticationType.None);

        appBuilder.UseAuthentication();

        appBuilder.UseRouting();

        //The call to UseCors must be placed after UseRouting, but before UseAuthorization
        appBuilder.UseCors();

        appBuilder.UseAuthorization();

        appBuilder.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}