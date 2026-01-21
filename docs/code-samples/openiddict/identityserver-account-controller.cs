// IdentityServer Account Controller - Login & Logout
// Simple login flow (direct authentication on IdentityServer)
// No OAuth flow needed for internal systems or quick setup
//
// Configuration in Program.cs:
// builder.Services.Configure<OidcClientSettings>(builder.Configuration.GetSection("OpenIddictClients"));

// Login method
[HttpGet]
public IActionResult Login(string? returnUrl = null)
{
    if (User.Identity!.IsAuthenticated)
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

    await _signInManager.SignInAsync(user, model.RememberMe);

    if (Url.IsLocalUrl(returnUrl))
    {
        return Redirect(returnUrl);
    }

    return RedirectToAction(nameof(HomeController.Index), "Home");
}

// Logout method with front-channel signout
// Signs out user from IdentityServer and all connected OIDC clients
public async Task<IActionResult> Logout()
{
    if (!User.Identity!.IsAuthenticated)
        return RedirectToAction("Login", "Account");

    await _signInManager.SignOutAsync();

    // Generate front-channel signout images for all registered clients
    var imgs = string.Join("\n",
        _oidcClientSettings.Clients.Values
            .Where(c => !string.IsNullOrWhiteSpace(c.BaseUrl))
            .Select(c =>
                $"<img src='{c.BaseUrl.TrimEnd('/')}/signout-oidc' style='display:none;' />"
            )
    );

    // Front-channel signout with redirect to login
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

// Dependencies
private readonly SignInManager<AppUser> _signInManager;
private readonly UserManager<AppUser> _userManager;
private readonly IStringLocalizer<SharedResource> _localizer;
private readonly OidcClientSettings _oidcClientSettings;

// OidcClientSettings for client management
public class OidcClientSettings
{
    public IDictionary<string, OidcClientConfiguration> Clients { get; set; } = new Dictionary<string, OidcClientConfiguration>();
}

public class OidcClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
}

// Constructor with configuration injection
public AccountController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    IStringLocalizer<SharedResource> localizer,
    IOptions<OidcClientSettings> oidcClientSettings)
{
    _signInManager = signInManager;
    _userManager = userManager;
    _localizer = localizer;
    _oidcClientSettings = oidcClientSettings.Value;
}
