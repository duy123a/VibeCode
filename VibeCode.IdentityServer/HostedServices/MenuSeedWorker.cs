using Microsoft.EntityFrameworkCore;
using VibeCode.IdentityServer.Data;
using VibeCode.Shared.Entities.Auth;

namespace VibeCode.IdentityServer.HostedServices;

public sealed class MenuSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public MenuSeedWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        // 1. Ensure the Parent Header exists
        // Root items: No ParentId, no URL (usually), serves as a folder
        var commonHeader = await EnsureMenuAsync(context, new Menu
        {
            Name = "Common",
            Icon = "bi bi-folder",
            TargetApp = MenuTargetApp.Identity,
            DisplayOrder = 1,
            Url = null, // Folders don't have URLs in our logic
            RequiredPermissionCode = null // Visible if children are visible
        });

        // 2. Ensure the Privacy Link exists under "Common"
        await EnsureMenuAsync(context, new Menu
        {
            Name = "Privacy",
            Url = "Home/Privacy", // This will become /auth/Home/Privacy via our logic
            Icon = "bi bi-shield-lock",
            TargetApp = MenuTargetApp.Identity,
            ParentId = commonHeader.Id,
            RequiredPermissionCode = "Privacy.Read", // Tied to your existing permission
            DisplayOrder = 1
        });

        await context.SaveChangesAsync();
    }

    private static async Task<Menu> EnsureMenuAsync(AuthDbContext context, Menu menu)
    {
        // Check if menu exists by Name and ParentId to avoid duplicates
        var existingMenu = await context.Menus
            .FirstOrDefaultAsync(m => m.Name == menu.Name && m.ParentId == menu.ParentId);

        if (existingMenu != null)
        {
            // Update existing record to match seed data (optional, but good for sync)
            existingMenu.Url = menu.Url;
            existingMenu.Icon = menu.Icon;
            existingMenu.TargetApp = menu.TargetApp;
            existingMenu.RequiredPermissionCode = menu.RequiredPermissionCode;
            existingMenu.DisplayOrder = menu.DisplayOrder;

            return existingMenu;
        }

        context.Menus.Add(menu);
        await context.SaveChangesAsync(); // Save to get the ID for children
        return menu;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}