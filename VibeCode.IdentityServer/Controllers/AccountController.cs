using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using VibeCode.IdentityServer.Models;
using VibeCode.IdentityServer.Settings;
using VibeCode.Shared.Entities;
using VibeCode.Shared.Resources;

namespace VibeCode.IdentityServer.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly CookieSettings _cookieSettings;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        IStringLocalizer<SharedResources> localizer,
        IOptions<CookieSettings> cookieSettings)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _localizer = localizer;
        _cookieSettings = cookieSettings.Value;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, _localizer["Login_Failed"]);
            return View(model);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError(string.Empty, _localizer["Login_Locked_Out"]);
            return View(model);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError(string.Empty, _localizer["Login_Not_Allowed"]);
            return View(model);
        }

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(_cookieSettings.RememberMeExpireDays)
                    : DateTimeOffset.UtcNow.AddSeconds(_cookieSettings.DefaultExpireSeconds)
        };

        await _signInManager.SignInAsync(user, model.RememberMe);

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        var clients = _configuration.GetSection("OpenIddictClients").Get<Dictionary<string, ClientConfig>>() ?? new();

        var imgs = string.Join("\n",
            clients.Values
                .Where(c => !string.IsNullOrWhiteSpace(c.BaseUrl))
                .Select(c =>
                    $"<img src='{c.BaseUrl.TrimEnd('/')}/signout-callback-oidc' style='display:none;' />"
                )
        );

        var html = $@"
            <html><body>
                {imgs}
                <script>
                    setTimeout(() => window.location='{_cookieSettings.CookiePath.TrimEnd('/')}{_cookieSettings.LoginPath}', 500);
                </script>
            </body></html>";

        Response.Headers["Cache-Control"] = "no-store";

        return Content(html, "text/html");
    }

    public class ClientConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
    }
}
