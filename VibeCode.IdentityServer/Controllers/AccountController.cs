using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VibeCode.IdentityServer.Models;
using VibeCode.Shared.Entities;

namespace VibeCode.IdentityServer.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
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
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

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

        var clients = _configuration.GetSection("OpenIddictClients:Clients").Get<Dictionary<string, ClientConfig>>() ?? new();

        var imgs = string.Join("\n",
            clients.Values
                .Where(c => !string.IsNullOrWhiteSpace(c.BaseUrl))
                .Select(c =>
                    $"<img src='{c.BaseUrl.TrimEnd('/')}/signout-oidc' style='display:none;' />"
                )
        );

        var html = $@"
            <html><body>
                {imgs}
                <script>
                    setTimeout(() => window.location='/Account/Login', 500);
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
