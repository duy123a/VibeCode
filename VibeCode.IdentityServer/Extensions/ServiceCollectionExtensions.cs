using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Services;
using VibeCode.IdentityServer.Services.Interface;
using VibeCode.IdentityServer.Settings;
using VibeCode.Shared.Entities;

namespace VibeCode.IdentityServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddErrorDescriber<AppErrorDescriber>()
        .AddDefaultTokenProviders();

        // Ensure the default authentication scheme is the Identity cookie.
        // This prevents the OpenIddict server handler from being selected for non-OpenIddict endpoints
        // (e.g. custom APIs like /api/notification/*).
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        });

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(10);
        });

        services.UseCookieAuthentication(configuration);

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });

        services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddAuthorization();

        services.Configure<OpenIddictClientSettings>(
            configuration.GetSection("OpenIddictClients:Main"));

        services.Configure<SeedUserSettings>(
            configuration.GetSection("SeedUsers"));

        return services;
    }
}
