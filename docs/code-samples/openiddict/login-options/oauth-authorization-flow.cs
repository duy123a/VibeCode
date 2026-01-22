// Full OAuth 2.0 authorization code flow with PKCE
// Main app redirects to IdentityServer for authentication
// Tokens are exchanged and user session is established in Main app
// HTTPS required for secure cookie handling

// Main app Program.cs - Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "OpenIddict";
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect("OpenIddict", options =>
{
    options.Authority = builder.Configuration["OpenIddictClients:IdentityServer:BaseUrl"]!;

    options.ClientId = builder.Configuration["OpenIddictClients:Main:ClientId"]!;
    options.ClientSecret = builder.Configuration["OpenIddictClients:Main:ClientSecret"]!;
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;

    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    options.RequireHttpsMetadata = true;
    options.UsePkce = true;

    // Scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");

    options.GetClaimsFromUserInfoEndpoint = false;

    options.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

    // Simplified event handlers
    options.Events = new OpenIdConnectEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/");
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// Main app Controller - Login/Logout
public IActionResult Login(string? returnUrl = null)
{
    var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
    return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, "OpenIddict");
}

public async Task<IActionResult> Logout()
{
    try
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        "OpenIddict", CookieAuthenticationDefaults.AuthenticationScheme);
    }
    catch (Exception)
    {
        // If logout fails (e.g., IdentityServer unreachable), still sign out locally
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}

// Protected page example
[Authorize]
public IActionResult SecurePage()
{
    // User is authenticated via OpenIddict
    return View();
}

// Admin-only page example
[Authorize(Roles = "Admin")]
public IActionResult AdminPage()
{
    // User is authenticated and has Admin role
    return View();
}
