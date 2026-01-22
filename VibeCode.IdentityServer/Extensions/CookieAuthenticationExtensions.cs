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
            options.ExpireTimeSpan = TimeSpan.FromSeconds(cookieSettings.DefaultExpireSeconds);
            options.SlidingExpiration = cookieSettings.SlidingExpiration;
        });

        return services;
    }

    public class CookieSettings
    {
        public string LoginPath { get; set; } = "/Account/Login";
        public string LogoutPath { get; set; } = "/Account/Logout";
        public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";
        public int DefaultExpireSeconds { get; set; } = 3600;
        public bool SlidingExpiration { get; set; } = true;
    }
}
