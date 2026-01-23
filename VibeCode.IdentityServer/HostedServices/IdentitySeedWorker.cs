using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VibeCode.IdentityServer.Settings;
using VibeCode.Shared.Entities;

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

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        var admin = await userManager.FindByEmailAsync(_seedOptions.AdminEmail);
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
            admin, _seedOptions.AdminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<AppRole> roleManager,
        string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
