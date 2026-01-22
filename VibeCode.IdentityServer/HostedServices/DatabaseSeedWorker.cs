using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using VibeCode.IdentityServer.Data;
using VibeCode.Shared.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VibeCode.IdentityServer.HostedServices;

public class DatabaseSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseSeedWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        await context.Database.MigrateAsync(cancellationToken);

        var mainClientId = "vibecode-main";
        var mainClient = await manager.FindByClientIdAsync(mainClientId, cancellationToken);

        if (mainClient == null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = mainClientId,
                ClientSecret = "d3f85c3b-4a7e-4f2d-9b1a-8c6d7e5f4a3b",
                DisplayName = "VibeCode Main Application",
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                },
                ClientType = ClientTypes.Confidential
            };

            var redirectUris = new HashSet<Uri>
            {
                new Uri("https://localhost:5001/signin-oidc")
            };
            foreach (var uri in redirectUris)
            {
                descriptor.RedirectUris.Add(uri);
            }

            var postLogoutUris = new HashSet<Uri>
            {
                new Uri("https://localhost:5001/signout-callback-oidc")
            };
            foreach (var uri in postLogoutUris)
            {
                descriptor.PostLogoutRedirectUris.Add(uri);
            }

            await manager.CreateAsync(descriptor, cancellationToken);
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await userManager.FindByEmailAsync("admin@example.com");

        if (admin == null)
        {
            admin = new AppUser
            {
                Email = "admin@example.com",
                UserName = "admin@example.com",
                EmailConfirmed = true,
                DisplayName = "Administrator",
                ProfileImg = ""
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Admin" });
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "User" });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
