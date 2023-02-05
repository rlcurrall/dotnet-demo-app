using Acme.Data.Interfaces;
using Acme.Common.Models.Weather;
using Acme.Common.Requests.Weather;
using Acme.Core.Extensions;
using Acme.Core.Models.AppSettings;
using Acme.Core.ServiceResponse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;

// using Acme.Core.Helpers;

namespace Acme.Data.Repositories;

/// <summary>
/// Snowflake Database interactions for Weather data.
/// </summary>
public class WeatherRepository : IWeatherRepository
{
    private readonly ILogger<WeatherRepository> _logger;
    private readonly SnowflakeAccessCredentials _weatherCredentials;

    private const string GetWeatherForecastSql = "Acme.Data.Scripts.Weather.GetWeatherForecast.sql";

    public WeatherRepository(IConfiguration configuration, ILogger<WeatherRepository> logger)
    {
        _logger = logger;
        _weatherCredentials = configuration.GetSection("Snowflake:Weather").Get<SnowflakeAccessCredentials>();
    }

    public async Task<ServiceResponse<WeatherForecast>> GetWeatherForecast(string bearerToken,
        GetWeatherForecastRequest request)
    {
        SnowflakeDbConnection? connection = null;

        try
        {
            // var sql = EmbeddedResourceHelper.GetEmbeddedResource(GetWeatherForecastSql);
            //
            // connection = _weatherCredentials.TokenConnect(bearerToken);
            //
            // WeatherForecast weather;
            //
            // await using (connection)
            // {
            //     _logger.LogInformation("Opening Snowflake Database Connection.");
            //     await connection.OpenAsync();
            //
            //     weather = await connection.QueryFirstAsync<WeatherForecast>(sql);
            // }

            var weather = new WeatherForecast
            {
                CloudCoverage = "Partly Cloudy",
                PercentToRain = 40,
                Temperature = 63
            };

            return weather.CreateServiceResponse();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"{exception.Message} | {exception.InnerException?.Message}");

            return ServiceResponse<WeatherForecast>.CreateErrorResponse(exception.InnerException!.Message);
        }
        finally
        {
            connection?.Disconnect(_logger);
        }
    }
}