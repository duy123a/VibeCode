namespace VibeCode.Shared.Entities.Auth
{
    public class UserPermission
    {
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
