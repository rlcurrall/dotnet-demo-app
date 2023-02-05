using Microsoft.AspNetCore.Mvc;

namespace Acme.Core.Controllers;

/// <summary>
///     Ensures that any controllers who inherit this class consume and return JSON.
/// </summary>
[Produces("application/json")]
[Route("api/[controller]")]
public abstract class ApiBase : Controller
{
}