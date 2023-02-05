using Acme.Common.Models.Weather;
using Acme.Common.Requests.Weather;
using Acme.Common.Responses.Weather;
using Acme.Services.Interfaces;
using Acme.Core.ServiceResponse;
using Acme.Data.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Acme.Services.Services;

public class WeatherService : IWeatherService
{
    private readonly ILogger<WeatherService> _logger;

    private readonly IWeatherRepository _weatherRepository;

    public WeatherService(IWeatherRepository weatherRepository, ILogger<WeatherService> logger)
    {
        _weatherRepository = weatherRepository;
        _logger = logger;
    }

    public async Task<ServiceResponse<GetWeatherForecastResponse>> GetWeatherForecast(string bearerToken, GetWeatherForecastRequest request)
    {
        var weather = await _weatherRepository.GetWeatherForecast(bearerToken, request);

        if (weather.Failed)
        {
            _logger.LogError(weather.Error.ApplicationErrorMessage);

            return ServiceResponseExtensions.CreateErrorResponse<GetWeatherForecastResponse>(
                "Failed to retrieve Weather Forecast.");
        }

        if (weather.IsNull)
        {
            _logger.LogError("Returned Weather Forecast was null.");

            return ServiceResponseExtensions.CreateNullServiceResponse<GetWeatherForecastResponse>(null);
        }

        GetWeatherForecastResponse response = new()
        {
            Forecast = weather.ResultData.Adapt<WeatherForecast>(),
            ForecastIssueDate = request.Date
        };

        return response.CreateServiceResponse();
    }
}