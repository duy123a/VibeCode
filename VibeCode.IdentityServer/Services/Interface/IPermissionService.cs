using VibeCode.Shared.Entities.Auth;

namespace VibeCode.IdentityServer.Services.Interface
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetPermissionsAsync(string userId);

        Task<List<Menu>> GetVisibleMenusAsync(IEnumerable<string> userPermissions);
    }
}
