using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using VibeCode.Main.Data;
using VibeCode.Main.Settings;
using VibeCode.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Enable PII logging only in development
if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MainDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAppLocalization();
builder.Services.AddControllersWithViews(o => o.AddStringTrimModelBinderProvider());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "OpenIddict";
})
.AddCookie(options =>
{
    var cookieSettings = builder.Configuration.GetSection("CookieSettings").Get<CookieSettings>();
    if (cookieSettings != null)
    {
        options.AccessDeniedPath = cookieSettings.AccessDeniedPath;
        options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSettings.DefaultExpireSeconds);
        options.SlidingExpiration = cookieSettings.SlidingExpiration;
        options.Cookie.Path = cookieSettings.CookiePath;

        // Allow front channel logout for OpenID Connect
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
})
.AddOpenIdConnect("OpenIddict", options =>
{
    options.Authority = builder.Configuration["OpenIddictClients:IdentityServer:BaseUrl"];
    options.ClientId = builder.Configuration["OpenIddictClients:Main:ClientId"];
    options.ClientSecret = builder.Configuration["OpenIddictClients:Main:ClientSecret"];

    var oidcSettings = builder.Configuration.GetSection("OpenIdConnectSettings").Get<OpenIdConnectSettings>();
    if (oidcSettings != null)
    {
        options.ResponseType = oidcSettings.ResponseType;
        options.UsePkce = oidcSettings.UsePkce;
        options.SaveTokens = oidcSettings.SaveTokens;
        options.CallbackPath = oidcSettings.CallbackPath;
        options.SignedOutCallbackPath = oidcSettings.SignedOutCallbackPath;
        options.RequireHttpsMetadata = oidcSettings.RequireHttpsMetadata;
        options.GetClaimsFromUserInfoEndpoint = oidcSettings.GetClaimsFromUserInfoEndpoint;

        options.Scope.Clear();
        foreach (var scope in oidcSettings.Scopes)
        {
            options.Scope.Add(scope);
        }

        options.Events = new OpenIdConnectEvents
        {
            OnRemoteFailure = context =>
            {
                var errorMessage = context.Failure?.Message;
                context.HandleResponse();
                context.Response.Redirect($"{context.Request.PathBase}{oidcSettings.FailureRedirectPath}?message={Uri.EscapeDataString(errorMessage ?? "Unknown error")}");
                return Task.CompletedTask;
            }
        };
    }

    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    options.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    db.Database.Migrate();
}

app.UsePathBase("/main");
if (!app.Environment.IsDevelopment())
{
    app.UseWhen(
        ctx => !ctx.Request.Path.StartsWithSegments("/api")
            && !ctx.Request.Path.StartsWithSegments("/connect"),
        appBuilder =>
        {
            appBuilder.UseExceptionHandler("/Home/Error");
            appBuilder.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
        });
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseRequestLocalization();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
