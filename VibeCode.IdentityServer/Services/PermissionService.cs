using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCode.IdentityServer.Data;
using VibeCode.IdentityServer.Services.Interface;
using VibeCode.Shared.Entities;

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
    }
}
