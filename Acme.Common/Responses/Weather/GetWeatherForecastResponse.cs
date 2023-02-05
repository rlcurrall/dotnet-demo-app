using Acme.Common.Models.Weather;

namespace Acme.Common.Responses.Weather;

public class GetWeatherForecastResponse
{
    public WeatherForecast Forecast { get; set; }

    public DateTime ForecastIssueDate { get; set; }
}