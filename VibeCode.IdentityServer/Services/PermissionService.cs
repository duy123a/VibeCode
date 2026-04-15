using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Services.Interface;
using VibeCode.Shared.Constants;
using VibeCode.Shared.Entities;
using VibeCode.Shared.Entities.Auth;

namespace VibeCode.IdentityServer.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PermissionService(
            AuthDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<HashSet<string>> GetPermissionsAsync(string userId)
        {
            var permissions = new HashSet<string>();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return permissions;

            // ROLE PERMISSIONS
            var roles = await _userManager.GetRolesAsync(user);

            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.Role.Name != null && roles.Contains(rp.Role.Name))
                .Select(rp => rp.Permission.Code)
                .ToListAsync();

            foreach (var p in rolePermissions)
                permissions.Add(p);

            // USER PERMISSIONS
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.Code)
                .ToListAsync();

            foreach (var p in userPermissions)
                permissions.Add(p);

            return permissions;
        }

        public async Task<List<Menu>> GetVisibleMenusAsync(IEnumerable<string> userPermissions)
        {
            var allMenus = await _context.Menus
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            // Now we use the list passed from the Claims, NO extra DB calls for permissions!
            var authorizedItems = allMenus.Where(m =>
                string.IsNullOrEmpty(m.RequiredPermissionCode) ||
                userPermissions.Contains(m.RequiredPermissionCode)
            ).ToList();

            return BuildAndPruneTree(authorizedItems, null);
        }

        private List<Menu> BuildAndPruneTree(List<Menu> source, int? parentId)
        {
            var branch = new List<Menu>();
            var items = source.Where(m => m.ParentId == parentId).ToList();

            foreach (var item in items)
            {
                // Recursively find children first
                var children = BuildAndPruneTree(source, item.Id);

                bool hasChildren = children.Any();
                bool isRoot = item.ParentId == null;

                // Apply your specific conditions:
                // 1. No parent (Root) BUT has children (It's a folder/header)
                // 2. Has parent BUT no children (It's a leaf link)
                bool shouldShow = (isRoot && hasChildren) || (!isRoot && !hasChildren);

                if (shouldShow)
                {
                    branch.Add(new Menu
                    {
                        Id = item.Id,
                        Name = item.Name,
                        // Prefix URL with /auth or /main based on TargetApp
                        Url = FormatUrl(item.Url, item.TargetApp),
                        Icon = item.Icon,
                        TargetApp = item.TargetApp,
                        DisplayOrder = item.DisplayOrder,
                        ParentId = item.ParentId,
                        Children = children
                    });
                }
            }

            return branch.OrderBy(m => m.DisplayOrder).ToList();
        }

        private static string? FormatUrl(string? url, MenuTargetApp targetApp)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            var cleanUrl = url.StartsWith('/') ? url : $"/{url}";
            var prefix = targetApp == MenuTargetApp.Identity ? AppConstants.AuthPath : AppConstants.MainPath;
            prefix = prefix.StartsWith('/') ? prefix : $"/{prefix}";

            return $"{prefix}{cleanUrl}";
        }
    }
}
