using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Settings;
using VibeCode.Shared.Entities;
using VibeCode.Shared.Entities.Auth;

namespace VibeCode.IdentityServer.HostedServices;

public sealed class IdentitySeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SeedUserSettings _seedOptions;

    public IdentitySeedWorker(
        IServiceProvider serviceProvider,
        IOptions<SeedUserSettings> seedOptions)
    {
        _serviceProvider = serviceProvider;
        _seedOptions = seedOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        await EnsurePermissionsAsync(context);

        await EnsureAdminUserAsync(userManager);
    }

    private static async Task EnsureRoleAsync(
        RoleManager<AppRole> roleManager,
        string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper()
            });
        }
    }

    private static async Task EnsurePermissionsAsync(AuthDbContext context)
    {
        var basePermissions = new[]
        {
            new Permission
            {
                Code = "Read",
                Name = "Read",
                Description = "Read permission"
            },
            new Permission
            {
                Code = "Write",
                Name = "Write",
                Description = "Write permission"
            },
            new Permission
            {
                Code = "Modify",
                Name = "Modify",
                Description = "Modify permission"
            },
            new Permission
            {
                Code = "Delete",
                Name = "Delete",
                Description = "Delete permission"
            }
        };

        foreach (var permission in basePermissions)
        {
            if (!await context.Permissions
                .AnyAsync(x => x.Code == permission.Code))
            {
                context.Permissions.Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task EnsureAdminUserAsync(
        UserManager<AppUser> userManager)
    {
        var admin = await userManager
            .FindByEmailAsync(_seedOptions.AdminEmail);

        if (admin != null)
            return;

        admin = new AppUser
        {
            Email = _seedOptions.AdminEmail,
            UserName = _seedOptions.AdminEmail,
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };

        var result = await userManager.CreateAsync(
            admin,
            _seedOptions.AdminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
