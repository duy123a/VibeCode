namespace VibeCode.Shared.Entities.Auth
{
    public class RolePermission
    {
        public string RoleId { get; set; } = null!;
        public AppRole Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
