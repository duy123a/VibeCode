// Simple login page on IdentityServer (direct authentication, no OAuth flow)
// Use this for quick setup or internal systems where OAuth isn't needed

// Controller in IdentityServer
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

// Program.cs configuration in IdentityServer
builder.Services.AddInfrastructureServices(builder.Configuration);
// No OpenIddict client configuration needed for simple login
