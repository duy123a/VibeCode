using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using VibeCode.Main.Data;
using VibeCode.Main.Settings;
using VibeCode.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Enable PII logging
IdentityModelEventSource.ShowPII = true;

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<VibeCodeDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAppLocalization();
builder.Services.AddControllersWithViews();

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
        options.LoginPath = cookieSettings.LoginPath;
        options.LogoutPath = cookieSettings.LogoutPath;
        options.AccessDeniedPath = cookieSettings.AccessDeniedPath;
        options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSettings.DefaultExpireSeconds);
        options.SlidingExpiration = cookieSettings.SlidingExpiration;
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
            OnAuthenticationFailed = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(oidcSettings.FailureRedirectPath);
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                context.Response.Redirect(oidcSettings.FailureRedirectPath);
                context.HandleResponse();
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
