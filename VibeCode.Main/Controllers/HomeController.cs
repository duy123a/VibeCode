using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VibeCode.Main.Models;

namespace VibeCode.Main.Controllers;

[Authorize]
public class HomeController : Controller
{
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
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
