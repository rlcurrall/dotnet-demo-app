namespace Acme.Common.Models.Weather;

public class WeatherForecast
{
    public int Temperature { get; set; }

    public string? CloudCoverage { get; set; }

    public int PercentToRain { get; set; }
}