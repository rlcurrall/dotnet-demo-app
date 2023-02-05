using Acme.Common.Requests.Weather;
using Acme.Common.Responses.Weather;
using Acme.Core.ServiceResponse;

namespace Acme.Services.Interfaces;

public interface IWeatherService
{
    Task<ServiceResponse<GetWeatherForecastResponse?>> GetWeatherForecast(string bearerToken,
        GetWeatherForecastRequest request);
}