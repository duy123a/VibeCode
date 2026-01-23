using VibeCode.IdentityServer.Settings;

namespace VibeCode.IdentityServer.Extensions;

public static class CookieAuthenticationExtensions
{
    public static IServiceCollection UseCookieAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var cookieSettings = configuration.GetSection("CookieSettings").Get<CookieSettings>()
                 ?? throw new InvalidOperationException("CookieSettings section is missing.");

        services.Configure<CookieSettings>(configuration.GetSection("CookieSettings"));

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = cookieSettings.LoginPath;
            options.LogoutPath = cookieSettings.LogoutPath;
            options.AccessDeniedPath = cookieSettings.AccessDeniedPath;
            options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSettings.DefaultExpireSeconds);
            options.SlidingExpiration = cookieSettings.SlidingExpiration;
        });

        return services;
    }
}
