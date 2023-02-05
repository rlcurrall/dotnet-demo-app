using System.Net;
using System.Text;
using Acme.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeMapping;
using Swashbuckle.AspNetCore.Annotations;

namespace Acme.Core.Controllers;

/// <summary>
///     Diagnostics controller for testing api interactions.
/// </summary>
[ApiController]
[Route("diagnostics")]
public class DiagnosticsController : ApiBase
{
    private readonly string _appName;

    private readonly string _appVersion;

    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _appName = configuration.GetValue<string>("AppName");
        _appVersion = configuration.GetValue<string>("AppVersion");
    }

    /// <summary>
    ///     Diagnostics - Connectivity Test
    /// </summary>
    /// <description>
    ///     This endpoint returns a 200 for testing if the API is live and receiving requests.
    /// </description>
    /// <returns>OK - 200</returns>
    [AllowAnonymous]
    [HttpGet("echo")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    public async Task<IActionResult> Echo()
    {
        _logger.LogInformation("Diagnostic Echo");
        _logger.LogError("Diagnostic Echo");
        return Ok("Echo Successful");
    }

    /// <summary>
    ///     Diagnostics - Swagger 2 Spec Download
    /// </summary>
    /// <description>
    ///     This endpoint returns a downloaded JSON spec sheet for the API as a Swagger 2 JSON File.
    /// </description>
    /// <returns>OK - 200</returns>
    [AllowAnonymous]
    [HttpGet("swagger2")]
    [Produces("application/json", Type = typeof(FileResult))]
    public async Task<IActionResult> GetSwagger2File()
    {
        var request = ControllerContext.HttpContext.Request;

        var host = request.Host.ToUriComponent();

        var swagger2Path = $"{request.Scheme}://{host}/swagger2/{_appVersion}/swagger.json";

        byte[] fileBytes;

        using (WebClient wc = new())
        {
            fileBytes = Encoding.UTF8.GetBytes(wc.DownloadString(swagger2Path));
        }

        var fileName = $"{_appName} Swagger2 {DateTime.Now:yyyy-MM-dd h:mm:ss tt}.json";

        return File(fileBytes, MimeUtility.GetMimeMapping(fileName), fileName);
    }

    /// <summary>
    ///     Diagnostics - OpenApi 3.0.1. Spec Download
    /// </summary>
    /// <description>
    ///     This endpoint returns a downloaded JSON spec sheet for the API as a OpenApi 3.0.1 JSON File.
    /// </description>
    /// <returns>OK - 200</returns>
    [AllowAnonymous]
    [HttpGet("openApi3")]
    [Produces("application/json", Type = typeof(FileResult))]
    public async Task<IActionResult> GetOpenApi3File()
    {
        var request = ControllerContext.HttpContext.Request;

        var host = request.Host.ToUriComponent();

        var swagger2Path = $"{request.Scheme}://{host}/swagger/{_appVersion}/swagger.json";

        byte[] fileBytes;

        using (WebClient wc = new())
        {
            fileBytes = Encoding.UTF8.GetBytes(wc.DownloadString(swagger2Path));
        }

        var fileName = $"{_appName} OpenApi3 {DateTime.Now:yyyy-MM-dd h:mm:ss tt}.json";

        return File(fileBytes, MimeUtility.GetMimeMapping(fileName), fileName);
    }

    /// <summary>
    ///     Diagnostics - Generate Test Log
    /// </summary>
    /// <description>
    ///     This endpoint generates a test log in Applications Insights.
    /// </description>
    /// <returns>OK - 200</returns>
    [SwaggerResponse(StatusCodes.Status200OK)]
    [HttpGet("test-log")]
    public async Task<IActionResult> LogBearerTest()
    {
        var token =
            AuthenticationHelper.ParseTokenHeaderValue(HttpContext.Request.Headers.Authorization.ToString(), _logger);

        _logger.LogInformation($"Diagnostics Controller captured the following Bearer Token: {token}.");

        _logger.LogInformation("Information - Test");
        _logger.LogWarning("Warning - Test");
        _logger.LogError("Error - Test");

        return Ok();
    }
}