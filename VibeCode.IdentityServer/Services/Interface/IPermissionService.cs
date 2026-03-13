namespace VibeCode.IdentityServer.Services.Interface
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetPermissionsAsync(string userId);
    }
}
