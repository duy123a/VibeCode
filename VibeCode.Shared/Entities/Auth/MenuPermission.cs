namespace VibeCode.Shared.Entities.Auth
{
    public class MenuPermission
    {
        public int MenuId { get; set; }
        public Menu Menu { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
