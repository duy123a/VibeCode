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
builder.Services.AddHostedService<MenuSeedWorker>();
builder.Services.AddHostedService<OpenIddictSeedWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UsePathBase("/auth");
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
app.Use(async (context, next) =>
{
    // Enforce PathBase for all routes.
    // If a request comes in without /auth, routing still matches but auth cookies (scoped to /auth)
    // won't be sent, causing redirects to /Account/Login with an incorrect ReturnUrl.
    if (string.IsNullOrEmpty(context.Request.PathBase) &&
        !context.Request.Path.StartsWithSegments("/auth", StringComparison.OrdinalIgnoreCase))
    {
        var target = "/auth" + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect(target, permanent: false);
        return;
    }

    await next();
});

app.UseRequestLocalization();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
