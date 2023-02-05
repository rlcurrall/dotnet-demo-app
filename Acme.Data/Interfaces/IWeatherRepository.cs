using Acme.Common.Models.Weather;
using Acme.Common.Requests.Weather;
using Acme.Core.ServiceResponse;

namespace Acme.Data.Interfaces
{
    public interface IWeatherRepository
    {
        public Task<ServiceResponse<WeatherForecast>> GetWeatherForecast(string bearerToken,
            GetWeatherForecastRequest request);
    }
}