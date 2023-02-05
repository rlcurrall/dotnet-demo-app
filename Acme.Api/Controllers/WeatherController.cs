using Acme.Common.Requests.Weather;
using Acme.Common.Responses.Weather;
using Acme.Core.Controllers;
using Acme.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Acme.Api.Controllers;

/// <summary>
///     Weather Controller for returning test data from Snowflake.
/// </summary>
[Authorize]
[ApiController]
[Route("weather")]
public class WeatherController : ApiBase
{
    private readonly ILogger<WeatherController> _logger;

    private readonly IWeatherService _weatherService;

    /// <summary>
    ///    Weather Controller
    /// </summary>
    /// <param name="weatherService"></param>
    /// <param name="logger"></param>
    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    ///     Weather - Get Weather
    /// </summary>
    /// <description>
    ///     Returns the first weather entry in the Weather Schema. Authorization Token is already set if
    ///     authenticated.
    /// </description>
    /// <returns>GetWeatherResponse</returns>
    [AllowAnonymous]
    [HttpPost("forecast")]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(GetWeatherForecastResponse))]
    public async Task<IActionResult> GetWeatherForecast(GetWeatherForecastRequest request)
    {
        //string token = AuthenticationHelper.ParseTokenHeaderValue(this.HttpContext.Request.Headers.Authorization.ToString(), _logger);

        //if (token.IsNullOrWhitespace())
        //{
        //    return StatusCode(StatusCodes.Status400BadRequest, "Authorization Token is invalid. Please authenticate through OAuth2.");
        //}

        const string token = "TestTokenToPassDemo";

        var weatherResponse =
            await _weatherService.GetWeatherForecast(token, request);

        if (weatherResponse.Failed)
        {
            _logger.LogError(weatherResponse.Error.ApplicationErrorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError,
                weatherResponse.Error.ApplicationErrorMessage);
        }

        if (weatherResponse.IsNull) return NotFound("No Weather Forecast could be found.");

        return Ok(weatherResponse.ResultData);
    }
}