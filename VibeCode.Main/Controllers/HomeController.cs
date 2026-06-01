using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VibeCode.Main.Models;

namespace VibeCode.Main.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [Authorize(Roles = "Admin")]
    [Route("debug/claims")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

        return Json(claims);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error(int? statusCode = null, string? message = null)
    {
        var model = new ErrorViewModel();

        if (!string.IsNullOrWhiteSpace(message) || statusCode.HasValue)
        {
            model.StatusCode = statusCode ?? 500;

            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            if (feature != null)
            {
                _logger.LogWarning(
                    "HTTP {StatusCode} error for path {Path}{Query}",
                    model.StatusCode,
                    feature.OriginalPath,
                    feature.OriginalQueryString);
            }

            model.Message =
                message
                ?? TempData["ErrorMessage"]?.ToString()
                ?? (statusCode switch
                {
                    404 => "Page not found.",
                    403 => "Access denied.",
                    401 => "Unauthorized.",
                    _ => "An unexpected error occurred."
                });

            return View(model);
        }

        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature != null)
        {
            _logger.LogError(exceptionFeature.Error, "Unhandled exception");

            model.StatusCode = 500;
            model.Message = _environment.IsDevelopment()
                ? exceptionFeature.Error.Message
                : "An unexpected error occurred.";
        }
        else
        {
            model.StatusCode = 500;
            model.Message = "Unknown error occurred.";
        }

        return View(model);
    }
}
