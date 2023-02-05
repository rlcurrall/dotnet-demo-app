namespace Acme.Common.Requests.Weather;

public class GetWeatherForecastRequest
{
    public DateTime Date { get; set; }

    public string Latitude { get; set; }

    public string Longitude { get; set; }
}