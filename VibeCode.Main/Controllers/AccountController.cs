using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        "OpenIddict", CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    public IActionResult AccessDenied(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Front-channel logout endpoint.
    [HttpGet("signout-oidc")]
    public async Task<IActionResult> SignOutOidc()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }
}
