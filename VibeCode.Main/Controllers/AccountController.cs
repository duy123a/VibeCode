using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace VibeCode.Main.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUri = returnUrl ?? "/";
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = redirectUri
            },
            "OpenIddict");
    }

    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync("OpenIddict");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
