using OpenIddict.Abstractions;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Extensions;
using VibeCode.IdentityServer.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllersWithViews();

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

        opt.SetAuthorizationEndpointUris("/connect/authorize")
           .SetTokenEndpointUris("/connect/token")
           .SetRevocationEndpointUris("/connect/revoke")
           .SetUserInfoEndpointUris("/connect/userinfo")
           .SetEndSessionEndpointUris("/connect/logout");

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

builder.Services.AddHostedService<DatabaseSeedWorker>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
