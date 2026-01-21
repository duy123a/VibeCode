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

[AllowAnonymous]
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
        ModelState.AddModelError(string.Empty, _localizer["Login_Locked"]);
        return View(model);
    }

    if (!await _userManager.IsEmailConfirmedAsync(user))
    {
        ModelState.AddModelError(string.Empty, _localizer["Login_Not_Allowed"]);
        return View(model);
    }

    if (!await _userManager.IsInRoleAsync(user, AppRole.Admin.ToString())
        && !await _userManager.IsInRoleAsync(user, AppRole.Staff.ToString()))
    {
        ModelState.AddModelError(string.Empty, _localizer["Login_Role_Not_Allowed"]);
        return View(model);
    }

    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = model.RememberMe
            ? DateTimeOffset.UtcNow.AddDays(14)
            : DateTimeOffset.UtcNow.AddSeconds(14400)
    };

    await _signInManager.SignInAsync(user, authProperties);

    if (Url.IsLocalUrl(returnUrl))
    {
        return Redirect(returnUrl);
    }

    return RedirectToAction(nameof(HomeController.Index), "Home");
}
