using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Extensions;
using VibeCode.IdentityServer.HostedServices;
using VibeCode.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAppLocalization();
builder.Services.AddControllersWithViews(o => o.AddStringTrimModelBinderProvider());

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddOpenIddict()
    .AddCore(opt =>
    {
        opt.UseEntityFrameworkCore()
           .UseDbContext<AuthDbContext>();
    })
    .AddServer(opt =>
    {
        // Issuer configuration
        var issuerUrl = builder.Configuration["OpenIddict:IssuerUrl"];
        if (!string.IsNullOrEmpty(issuerUrl))
        {
            opt.SetIssuer(new Uri(issuerUrl));
        }

        opt.SetAuthorizationEndpointUris("/auth/connect/authorize")
           .SetTokenEndpointUris("/auth/connect/token")
           .SetRevocationEndpointUris("/auth/connect/revoke")
           .SetUserInfoEndpointUris("/auth/connect/userinfo")
           .SetEndSessionEndpointUris("/auth/connect/logout");

        opt.AllowAuthorizationCodeFlow();

        opt.RequireProofKeyForCodeExchange();

        opt.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles
        );

        opt.AddEphemeralEncryptionKey()
           .AddEphemeralSigningKey();

        opt.DisableAccessTokenEncryption();

        opt.UseAspNetCore()
           .EnableAuthorizationEndpointPassthrough()
           .EnableTokenEndpointPassthrough()
           .EnableUserInfoEndpointPassthrough()
           .EnableEndSessionEndpointPassthrough();
    })
    .AddValidation(opt =>
    {
        opt.UseLocalServer();
        opt.UseAspNetCore();
    });

builder.Services.AddHostedService<IdentitySeedWorker>();
builder.Services.AddHostedService<OpenIddictSeedWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UsePathBase("/auth");
app.UseForwardedHeaders();

app.UseRequestLocalization();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
