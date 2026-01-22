using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCode.IdentityServer.Data;
using VibeCode.Shared.Entities;
using VibeCode.Shared.Interfaces;
using VibeCode.Shared.Repositories;

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
        .AddDefaultTokenProviders();

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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddAuthorization();

        return services;
    }
}
